package authserv

import (
	"github.com/golang/glog"
	"strconv"
	"github.com/markbates/goth"
	"lobby/servs/dbserv"
	"errors"
	"lobby/models"
)

var (
	ErrNotFound = errors.New("not found")
)

type AuthServ struct {
	DB *dbserv.DBServ `dim:"on"`
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

func (a *AuthServ) GetUser(id int) (models.User, error) {
	user := models.User{}
	err := a.DB.Q().Where("id = ?", id).First(&user)
	return user, err
}

func (a *AuthServ) CreateUserByOAuth(provider string, guser goth.User) (models.User, error) {
	username := guser.NickName
	if username == "" {
		username = guser.Name
	}
	user := models.User{
		Username: username,
		Picture: guser.AvatarURL,
	}
	err := a.DB.Create(&user)
	if err != nil {
		return models.User{}, err
	}
	ouser := models.OAuthUser {
		Provider: provider,
		ID: guser.UserID,
		UserID: user.ID,
	}
	err = a.DB.Create(&ouser)
	return user, err
}

func (a *AuthServ) GetUserByOAuth(provider string, id string) (models.User, error) {
	ouser := models.OAuthUser{}
	err := a.DB.Q().Where("provider = ? AND id = ?", provider, id).First(&ouser)
	if err != nil {
		return models.User{}, ErrNotFound
	}

	return a.GetUser(ouser.UserID)
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
		glog.Fatalf("Error while creating token %v", err)
	}
	return str
}

func (a *AuthServ) Authorize(token string) (models.User, error) {
	if token == "black" {
		return models.User{
			ID:       101,
			Username: "흑우",
		}, nil
	}
	if token == "white" {
		return models.User{
			ID:       201,
			Username: "백우",
		}, nil
	}

	id, err := a.ParseToken(token) 
	if err != nil {
		return models.User{}, err
	}

	return a.GetUser(id)
}
