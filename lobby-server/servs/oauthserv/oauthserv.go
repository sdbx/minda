package oauthserv

import (
	"lobby/servs/authserv"
	"encoding/base64"
	"github.com/gobuffalo/uuid"
	"errors"
	"net/url"
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

type OAuthServ struct {
	Auth *authserv.AuthServ `dim:"on"`
	Redis *redisserv.RedisServ `dim:"on"`
	reqs map[string]*string
}

type oauthProvier struct {
	Key    string `yaml:"key"`
	Secret string `yaml:"secret"`
}

type OAuthServConf struct {
	CallbackURL string        `yaml:"callback_url"`
	Naver       *oauthProvier `yaml:"naver"`
	Discord     *oauthProvier `yaml:"discord"`
	Google      *oauthProvier `yaml:"google"`
}

func Provide(conf OAuthServConf) *OAuthServ {
	providers := []goth.Provider{}
	if conf.Naver != nil {
		providers = append(providers, naver.New(conf.Naver.Key, conf.Naver.Secret, conf.CallbackURL+"/naver"))
	}
	if conf.Discord != nil {
		providers = append(providers, discord.New(conf.Discord.Key, conf.Discord.Secret, conf.CallbackURL+"/discord"))
	}
	if conf.Google != nil {
		providers = append(providers, google.New(conf.Google.Key, conf.Google.Secret, conf.CallbackURL+"/google"))
	}
	goth.UseProviders(providers...)
	return &OAuthServ{}
}

func (a *OAuthServ) Init() error {
	store, err := redistore.NewRediStoreWithPool(a.Redis.GetPool())
	if err != nil {
		return err
	}
	gothic.Store = store
	gothic.SetState = func(req *http.Request) string {
		state := req.URL.Query().Get("state")
		if len(state) > 0 {
			return state
		}
	
		id, _ := uuid.NewV4()
		return base64.URLEncoding.EncodeToString(id.Bytes())
	}
	return nil
}

func (a *OAuthServ) CompleteAuth(c echo.Context, provider string) (goth.User, error) {
	r := c.Request()
	r.URL.Query().Set("provider", provider)
	state := r.URL.Query().Get("state")
	if _, ok := a.reqs[state]; !ok {
		c.Error(echo.NewHTTPError(http.StatusBadRequest))
		return goth.User{}, errors.New("no such auth request")
	}

	user, err := gothic.CompleteUserAuth(c.Response().Writer,r)
	if err != nil {
		c.Error(echo.NewHTTPError(http.StatusBadRequest))
		return goth.User{}, err
	}

	return user, nil
}

func (a *OAuthServ) BeginAuth(c echo.Context, provider string) (string, error) {
	r := c.Request()
	r.URL.Query().Set("provider", provider)
	u, err := gothic.GetAuthURL(c.Response().Writer, r)
	if err != nil {
		c.Error(echo.NewHTTPError(http.StatusBadRequest))
		return "", err
	}
	u2, err  := url.Parse(u)
	if err != nil {
		c.Error(echo.NewHTTPError(http.StatusInternalServerError))
		return "", err
	}
	state := u2.Query().Get("state")
	if state == "" {
		c.Error(echo.NewHTTPError(http.StatusInternalServerError))
		return "", err
	}
	a.reqs[state] = nil
	return "", nil
}
