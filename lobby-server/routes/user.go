package routes

import (
	"lobby/models"
	"lobby/middlewares"

	"github.com/labstack/echo"
	"github.com/sunho/dim"
)

type user struct {
}

func (u *user) Register(d *dim.Group) {
	d.RouteFunc("/me", func(d *dim.Group) {
		d.Use(&middlewares.UserMiddleware{})
		d.GET("/", u.me)
	})
	d.GET("/:id/", u.getUser)
}

func (u *user) me(c2 echo.Context) error {
	c := c2.(*models.Context)
	return c.JSON(200, *c.User)
}

func (u *user) getUser(c echo.Context) error {
	return nil
}
