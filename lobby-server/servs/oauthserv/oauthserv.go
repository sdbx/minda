package oauthserv

import (
	"lobby/utils"
	"errors"
	"encoding/base64"
	"github.com/gofrs/uuid"
	"lobby/servs/authserv"
	"net/http"
	"github.com/labstack/echo"
	"github.com/markbates/goth/gothic"
	"lobby/servs/redisserv"
	"github.com/markbates/goth/providers/google"
	"github.com/markbates/goth/providers/discord"
	"github.com/markbates/goth"
	"github.com/markbates/goth/providers/naver"
	"gopkg.in/boj/redistore.v1"
)

var (
	ErrNotAuthorized = errors.New("not authorized")
)

type OAuthServ struct {
	Auth *authserv.AuthServ `dim:"on"`
	Redis *redisserv.RedisServ `dim:"on"`
	secret []byte
	reqs map[string]*string
}

type oauthProvier struct {
	Key    string `yaml:"key"`
	Secret string `yaml:"secret"`
}

type OAuthServConf struct {
	CallbackURL string        `yaml:"callback_url"`
	Secret string `yaml:"secret"`
	Naver       *oauthProvier `yaml:"naver"`
	Discord     *oauthProvier `yaml:"discord"`
	Google      *oauthProvier `yaml:"google"`
}

func Provide(conf OAuthServConf) *OAuthServ {
	providers := []goth.Provider{}
	if conf.Naver != nil {
		providers = append(providers, naver.New(conf.Naver.Key, conf.Naver.Secret, conf.CallbackURL+"/naver/"))
	}
	if conf.Discord != nil {
		providers = append(providers, discord.New(conf.Discord.Key, conf.Discord.Secret, conf.CallbackURL+"/discord/"))
	}
	if conf.Google != nil {
		providers = append(providers, google.New(conf.Google.Key, conf.Google.Secret, conf.CallbackURL+"/google/"))
	}
	goth.UseProviders(providers...)
	return &OAuthServ{
		secret: []byte(conf.Secret),
		reqs: make(map[string]*string),
	}
}

func (OAuthServ) ConfigName() string {
	return "oauth"
}

func (a *OAuthServ) Init() error {
	store, err := redistore.NewRediStoreWithPool(a.Redis.GetPool(), a.secret)
	if err != nil {
		return err
	}
	gothic.Store = store
	return nil
}

func (a *OAuthServ) List() []string {
	providers := goth.GetProviders() 
	out := make([]string, 0, len(providers))
	for name := range providers {
		out = append(out, name)
	}
	return out
}

func (a *OAuthServ) GetReq(reqid string) (string, error) {
	tok, ok := a.reqs[reqid]
	if !ok {
		return "", utils.ErrNotExists
	}
	if tok == nil {
		return "", ErrNotAuthorized
	}
	delete(a.reqs, reqid)
	return *tok, nil
}

func (a *OAuthServ) MakeReq() string {
	id, _ := uuid.NewV4()
	reqid := base64.URLEncoding.EncodeToString(id.Bytes())
	a.reqs[reqid] = nil
	return reqid
}

func (a *OAuthServ) CompleteAuth(c echo.Context, provider string) error {
	r := c.Request()
	q := r.URL.Query()
	q.Set("provider", provider)
    r.URL.RawQuery = q.Encode()
	reqid := q.Get("state")

	if _, ok := a.reqs[reqid]; !ok {
		return echo.NewHTTPError(http.StatusBadRequest)
	}

	guser, err := gothic.CompleteUserAuth(c.Response().Writer,r)
	if err != nil {
		return err
	}

	user, err := a.Auth.GetUserByOAuth(provider, guser.UserID)
	if err == authserv.ErrNotFound {
		user, err = a.Auth.CreateUserByOAuth(provider, guser)
		if err != nil {
			return err
		}
	} else if err != nil{
		return err
	}

	tok := a.Auth.CreateToken(user.ID)
	a.reqs[reqid] = &tok

	return c.JSON(200, user)
}

func (a *OAuthServ) BeginAuth(c echo.Context, provider string, reqid string) error {
	if _, ok := a.reqs[reqid]; !ok {
		return echo.NewHTTPError(http.StatusBadRequest)
	}

	r := c.Request()
	q := r.URL.Query()
	q.Set("provider", provider)
	q.Set("state", reqid)
	r.URL.RawQuery = q.Encode()

	u, err := gothic.GetAuthURL(c.Response().Writer, r)
	if err != nil {
		return err
	}

	return c.Redirect(302, u)
}
