package routes

import (
	"lobby/models"
	"lobby/servs/authserv"
	"lobby/servs/oauthserv"

	"github.com/labstack/echo"
	"github.com/sunho/dim"
)

type auth struct {
	OAuth *oauthserv.OAuthServ `dim:"on"`
	Auth  *authserv.AuthServ   `dim:"on"`
}

func (a *auth) Register(d *dim.Group) {
	d.GET("/reqs/:reqid/", a.getReq)
	d.POST("/reqs/", a.postReq)
	d.GET("/o/", a.listOauth)
	d.GET("/o/:provider/:reqid/", a.oauth)
	d.GET("/o/callback/:provider/", a.oauthCallback)
	d.GET("/steam/", a.getSteam)
}

func (a *auth) listOauth(c *models.Context) error {
	return c.JSON(200, a.OAuth.List())
}

func (a *auth) getReq(c *models.Context) error {
	req, err := a.OAuth.GetRequest(c.Param("reqid"))
	if err != nil {
		return err
	}
	return c.JSONPretty(200, req, "\t")
}

func (a *auth) postReq(c *models.Context) error {
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

func (a *auth) oauth(c *models.Context) error {
	return a.OAuth.BeginAuth(c, c.Param("provider"), c.Param("reqid"))
}

func (a *auth) oauthCallback(c *models.Context) error {
	return a.OAuth.CompleteAuth(c, c.Param("provider"))
}

func (a *auth) getSteam(c *models.Context) error {
	ticket := c.QueryParam("ticket")
	if ticket == "" {
		return echo.NewHTTPError(400, "Empty ticket")
	}

	req, err := a.Auth.AuthorizeBySteam(ticket)
	if err != nil {
		return err
	}
	return c.JSON(200, req)
}
