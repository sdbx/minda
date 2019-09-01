package oauthserv

import (
	"encoding/base64"
	"encoding/json"
	"errors"
	"fmt"
	"lobby/models"
	"lobby/servs/authserv"
	"lobby/servs/redisserv"
	"net/http"

	"github.com/garyburd/redigo/redis"
	"github.com/gofrs/uuid"
	"github.com/labstack/echo"
	"github.com/markbates/goth"
	"github.com/markbates/goth/gothic"
	"github.com/markbates/goth/providers/discord"
	"github.com/markbates/goth/providers/google"
	"github.com/markbates/goth/providers/naver"
	"github.com/markbates/goth/providers/steam"

	"gopkg.in/boj/redistore.v1"
)

var (
	ErrNotAuthorized = errors.New("not authorized")
)

const (
	sessionName          = "_minda_reqid"
	redisAuthRequestTmpl = "auth_request_%s"
	authRequestTimeout   = 180
)

type oauthProvier struct {
	Key    string `yaml:"key"`
	Secret string `yaml:"secret"`
}

type OAuthServConf struct {
	CallbackURL string        `yaml:"callback_url"`
	Secret      string        `yaml:"secret"`
	Naver       *oauthProvier `yaml:"naver"`
	Discord     *oauthProvier `yaml:"discord"`
	Google      *oauthProvier `yaml:"google"`
	Steam       *oauthProvier `yaml:"steam"`
}

type OAuthServ struct {
	Auth   *authserv.AuthServ   `dim:"on"`
	Redis  *redisserv.RedisServ `dim:"on"`
	secret []byte
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
	if conf.Steam != nil {
		providers = append(providers, steam.New(conf.Steam.Key, conf.CallbackURL+"/steam/"))
	}
	goth.UseProviders(providers...)
	return &OAuthServ{
		secret: []byte(conf.Secret),
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

func (a *OAuthServ) GetRequest(reqid string) (models.AuthRequest, error) {
	buf, err := redis.Bytes(a.Redis.Conn().Do("GET", redisAuthRequest(reqid)))
	if err != nil {
		return models.AuthRequest{}, err
	}
	var out models.AuthRequest
	err = json.Unmarshal(buf, &out)
	if out.Token == nil {
		return models.AuthRequest{}, ErrNotAuthorized
	}
	return out, err
}

func (a *OAuthServ) CreateRequest() (string, error) {
	id, _ := uuid.NewV4()
	reqid := base64.URLEncoding.EncodeToString(id.Bytes())
	err := a.setRequest(reqid, models.AuthRequest{})
	return reqid, err
}

func (a *OAuthServ) existsRequest(reqid string) (bool, error) {
	return redis.Bool(a.Redis.Conn().Do("EXISTS", redisAuthRequest(reqid)))
}

func (a *OAuthServ) setRequest(reqid string, req models.AuthRequest) error {
	buf, err := json.Marshal(req)
	if err != nil {
		return err
	}

	_, err = a.Redis.Conn().Do("SET", redisAuthRequest(reqid), buf)
	if err != nil {
		return err
	}
	_, err = a.Redis.Conn().Do("EXPIRE", redisAuthRequest(reqid), authRequestTimeout)
	return err
}

func (a *OAuthServ) CompleteAuth(c *models.Context, provider string) error {
	r := c.Request()
	q := r.URL.Query()
	q.Set("provider", provider)
	r.URL.RawQuery = q.Encode()

	sess, err := gothic.Store.Get(r, sessionName)
	if err != nil {
		return err
	}
	reqid2 := sess.Values["reqid"]
	if reqid2 == nil {
		return echo.NewHTTPError(http.StatusBadRequest)
	}
	reqid := reqid2.(string)

	exists, err := a.existsRequest(reqid)
	if err != nil {
		return err
	}
	if !exists {
		return echo.NewHTTPError(http.StatusBadRequest)
	}

	guser, err := gothic.CompleteUserAuth(c.Response().Writer, r)
	if err != nil {
		return err
	}

	req, err := a.Auth.AuthorizeByOAuth(provider, guser)
	if err != nil {
		return err
	}

	err = a.setRequest(reqid, req)
	if err != nil {
		return err
	}

	return c.NoContent(200)
}

func (a *OAuthServ) BeginAuth(c echo.Context, provider string, reqid string) error {
	exists, err := a.existsRequest(reqid)
	if err != nil {
		return err
	}
	if !exists {
		return echo.NewHTTPError(http.StatusBadRequest)
	}

	r := c.Request()
	q := r.URL.Query()
	q.Set("provider", provider)
	q.Set("state", reqid)
	r.URL.RawQuery = q.Encode()
	sess, _ := gothic.Store.New(r, sessionName)
	sess.Values["reqid"] = reqid
	sess.Save(r, c.Response().Writer)

	u, err := gothic.GetAuthURL(c.Response().Writer, r)
	if err != nil {
		return err
	}

	return c.Redirect(302, u)
}

func redisAuthRequest(reqid string) string {
	return fmt.Sprintf(redisAuthRequestTmpl, reqid)
}
