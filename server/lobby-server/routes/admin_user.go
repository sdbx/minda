package routes

import (
	"lobby/middlewares"
	"lobby/models"
	"lobby/servs/authserv"
	"lobby/servs/dbserv"

	"github.com/sunho/dim"
)

type adminUser struct {
	DB   *dbserv.DBServ     `dim:"on"`
	Auth *authserv.AuthServ `dim:"on"`
}

func (a *adminUser) Register(d *dim.Group) {
	d.GET("/", a.listUser)
	d.POST("/", a.postUser)
	d.RouteFunc("/:userid", func(d *dim.Group) {
		d.Use(&middlewares.UserMiddleware{})
		d.PUT("/", a.putUser)
		d.DELETE("/", a.deleteUser)
		d.POST("/token/", a.postUserToken)
	})
}

func (a *adminUser) listUser(c *models.Context) error {
	var out []models.User
	err := a.DB.Eager().All(&out)
	if err != nil {
		return err
	}
	return c.JSON(200, out)
}

func (a *adminUser) postUser(c *models.Context) error {
	var user models.User
	err := c.Bind(&user)
	if err != nil {
		return err
	}
	user.ID = 0
	err = a.DB.Eager().Create(&user)
	if err != nil {
		return err
	}
	return c.NoContent(201)
}

func (a *adminUser) putUser(c *models.Context) error {

	var user models.User
	err := c.Bind(&user)
	if err != nil {
		return err
	}
	user.ID = c.UserParam.ID

	err = a.DB.Eager().Save(&user)
	if err != nil {
		return err
	}

	return c.NoContent(200)
}

func (a *adminUser) deleteUser(c *models.Context) error {

	err := a.DB.Destroy(&c.UserParam)
	if err != nil {
		return err
	}
	return c.NoContent(200)
}

func (a *adminUser) postUserToken(c *models.Context) error {

	tok := a.Auth.CreateToken(c.UserParam.ID)
	return c.JSON(200, struct {
		Token string `json:"token"`
	}{
		Token: tok,
	})
}
