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
	req, err := a.OAuth.GetRequest(c.Param("reqid"))
	if err != nil {
		return err
	}
	return c.JSONPretty(200, req, "\t")
}

func (a *auth) postReq(c echo.Context) error {
	reqid, err := a.OAuth.CreateRequest()
	if err != nil {
		return err
	}
	return c.JSONPretty(201, struct {
		ReqID string `json:"req_id"`
	}{
		ReqID: reqid,
	}, "\t")
}

func (a *auth) oauth(c echo.Context) error {
	return a.OAuth.BeginAuth(c, c.Param("provider"), c.Param("reqid"))
}

func (a *auth) oauthCallback(c echo.Context) error {
	return a.OAuth.CompleteAuth(c, c.Param("provider"))
}
