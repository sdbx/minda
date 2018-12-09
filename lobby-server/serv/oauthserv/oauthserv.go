package oauthserv

import (
	"github.com/labstack/echo"
	"github.com/markbates/goth/gothic"
	"lobby/serv/redisserv"
	"github.com/markbates/goth/providers/google"
	"github.com/markbates/goth/providers/discord"
	"github.com/markbates/goth"
	"github.com/markbates/goth/providers/naver"
	"gopkg.in/boj/redistore.v1"
)

type OAuthServ struct {
	Redis *redisserv.RedisServ `dim:"on"`
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
	return nil
}

func (a *OAuthServ) CompleteAuth(c echo.Context, provider string) (goth.User, error) {
	r := c.Request()
	r.URL.Query().Set("provider", provider)
	return gothic.CompleteUserAuth(c.Response().Writer,r)
}

func (a *OAuthServ) BeginAuth(c echo.Context, provider string) {
	r := c.Request()
	r.URL.Query().Set("provider", provider)
	gothic.BeginAuthHandler(c.Response().Writer,r)
}
