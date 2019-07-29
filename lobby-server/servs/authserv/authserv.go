package authserv

import (
	"errors"
	"lobby/models"
	"lobby/servs/dbserv"
	"lobby/servs/picserv"
	"lobby/servs/steamserv"
	"lobby/utils"
	"net/http"
	"strconv"

	"github.com/gobuffalo/pop/nulls"
	"github.com/markbates/goth"
	"go.uber.org/zap"
)

var (
	ErrNotFound = errors.New("not found")
)

type AuthServ struct {
	Steam  *steamserv.SteamServ `dim:"on"`
	DB     *dbserv.DBServ       `dim:"on"`
	Pic    *picserv.PicServ     `dim:"on"`
	secret []byte
}

type AuthServConf struct {
	Secret string `yaml:"secret"`
}

func Provide(conf AuthServConf) *AuthServ {
	return &AuthServ{
		secret: []byte(conf.Secret),
	}
}

func (AuthServ) ConfigName() string {
	return "auth"
}

func (a *AuthServ) Init() error {
	size, err := a.DB.Count(&models.User{})
	if err != nil {
		return err
	}
	if size == 0 {
		user := models.User{
			Username: "admin",
			Picture:  nulls.String{Valid: false},
			Permission: models.UserPermission{
				Admin: true,
			},
		}
		err := a.DB.Eager().Create(&user)
		if err != nil {
			return err
		}
		utils.Log.Info("Admin token", zap.String("token", a.CreateToken(user.ID)))
	}
	return nil
}

func (a *AuthServ) GetUser(id int) (models.User, error) {
	user := models.User{}
	err := a.DB.Eager().Q().Where("id = ?", id).First(&user)
	return user, err
}

func (a *AuthServ) uploadImg(url string) (string, error) {
	resp, err := http.Get(url)
	if err != nil {
		return "", err
	}
	defer resp.Body.Close()
	img, err := a.Pic.ParseImage(resp.Body)
	if err != nil {
		return "", err
	}
	return a.Pic.UploadImage(img)
}

func (a *AuthServ) CreateUserByOAuth(provider string, guser goth.User) (models.User, error) {
	username := guser.NickName
	if username == "" {
		username = guser.Name
	}
	picture := nulls.String{Valid: false}
	if guser.AvatarURL != "" {
		img, err := a.uploadImg(guser.AvatarURL)
		if err != nil {
			utils.Log.Error("Error while uploading image", zap.Error(err))
		} else {
			picture = nulls.NewString(img)
		}
	}
	user := models.User{
		Username: username,
		Picture:  picture,
		Rating: models.UserRating{
			R:  1500,
			RD: 350,
			V:  0.06,
		},
	}
	err := a.DB.Eager().Create(&user)
	if err != nil {
		return models.User{}, err
	}
	ouser := models.OAuthUser{
		Provider: provider,
		ID:       guser.UserID,
		UserID:   user.ID,
	}
	err = a.DB.Create(&ouser)
	return user, err
}

func (a *AuthServ) AuthorizeByOAuth(provider string, guser goth.User) (models.AuthRequest, error) {
	var first bool

	user, err := a.GetUserByOAuth(provider, guser.UserID)
	if err == ErrNotFound {
		user, err = a.CreateUserByOAuth(provider, guser)
		first = true
		if err != nil {
			return models.AuthRequest{}, err
		}
	} else if err != nil {
		return models.AuthRequest{}, err
	}

	tok := a.CreateToken(user.ID)
	return models.AuthRequest{
		Token: &tok,
		First: first,
	}, nil
}

func (a *AuthServ) AuthorizeBySteam(ticket string) (models.AuthRequest, error) {
	id, err := a.Steam.AuthenticateUserTicket(ticket)
	if err != nil {
		return models.AuthRequest{}, err
	}

	user, err := a.Steam.GetPlayerSummary(id)
	if err != nil {
		return models.AuthRequest{}, err
	}

	guser := goth.User{
		UserID:    user.ID,
		Name:      user.Name,
		AvatarURL: user.AvatarMedium,
	}
	return a.AuthorizeByOAuth("steam", guser)
}

func (a *AuthServ) GetUserByOAuth(provider string, id string) (models.User, error) {
	ouser := models.OAuthUser{}
	err := a.DB.Q().Where("provider = ? AND id = ?", provider, id).First(&ouser)
	if err != nil {
		return models.User{}, ErrNotFound
	}

	return a.GetUser(ouser.UserID)
}

func (a *AuthServ) GetOAuthUserByUser(id int) (models.OAuthUser, error) {
	out := models.OAuthUser{}
	err := a.DB.Q().Where("user_id = ?", id).First(&out)
	if err != nil {
		return models.OAuthUser{}, ErrNotFound
	}

	return out, nil
}

func (a *AuthServ) ParseToken(token string) (int, error) {
	str, err := decrypt(a.secret, token)
	if err != nil {
		return 0, err
	}

	return strconv.Atoi(str)
}

func (a *AuthServ) CreateToken(id int) string {
	str, err := encrypt(a.secret, strconv.Itoa(id))
	if err != nil {
		utils.Log.Fatal("Error while creating token", zap.Error(err))
	}
	return str
}

func (a *AuthServ) Authorize(token string) (models.User, error) {
	id, err := a.ParseToken(token)
	if err != nil {
		return models.User{}, err
	}

	return a.GetUser(id)
}
