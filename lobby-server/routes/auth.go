package routes

import (
	"lobby/serv/oauthserv"

	"github.com/labstack/echo"
	"github.com/sunho/dim"
)

type auth struct {
	OAuth *oauthserv.OAuthServ `dim:"on"`
}

func (a *auth) Register(d *dim.Group) {
	d.GET()
	d.GET("/o/:provider", a.oauth)
	d.GET("/o/callback/:provider", a.oauthCallback)
}

func (a *auth) oauth(c echo.Context) error {
	a.OAuth.BeginAuth(c, c.Param("provider"))
	return nil
}

func (a *auth) oauthCallback(c echo.Context) error {
	a.OAuth.BeginAuth(c, c.Param("provider"))
	return nil
}
