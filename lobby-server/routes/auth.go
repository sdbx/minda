package routes

import (
	"lobby/servs/oauthserv"

	"github.com/labstack/echo"
	"github.com/sunho/dim"
)

type auth struct {
	OAuth *oauthserv.OAuthServ `dim:"on"`
}

func (a *auth) Register(d *dim.Group) {
	d.GET("/reqs/:reqid/", a.getReq)
	d.POST("/reqs/", a.postReq)
	d.GET("/o/", a.listOauth)
	d.GET("/o/:provider/:reqid/", a.oauth)
	d.GET("/o/callback/:provider/", a.oauthCallback)
}

func (a *auth) listOauth(c echo.Context) error {
	return c.JSON(200, a.OAuth.List())
}

func (a *auth) getReq(c echo.Context) error {
	tok, err := a.OAuth.GetReq(c.Param("reqid"))
	if err != nil {
		return err
	}
	return c.String(200, tok)
}

func (a *auth) postReq(c echo.Context) error {
	reqid := a.OAuth.MakeReq()
	return c.String(201, reqid)
}

func (a *auth) oauth(c echo.Context) error {
	return a.OAuth.BeginAuth(c, c.Param("provider"), c.Param("reqid"))
}

func (a *auth) oauthCallback(c echo.Context) error {
	return a.OAuth.CompleteAuth(c, c.Param("provider"))
}
