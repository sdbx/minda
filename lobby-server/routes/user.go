package routes

import (
	"github.com/gobuffalo/pop/nulls"
	"lobby/servs/picserv"
	"io/ioutil"
	"lobby/middlewares"
	"lobby/models"
	"lobby/servs/dbserv"
	"strconv"

	"github.com/labstack/echo"
	"github.com/sunho/dim"
)

type user struct {
	DB *dbserv.DBServ `dim:"on"`
	Pic *picserv.PicServ `dim:"on"`
}

func (u *user) Register(d *dim.Group) {
	d.RouteFunc("/me", func(d *dim.Group) {
		d.Use(&middlewares.AuthMiddleware{})
		d.GET("/", u.me)
		d.PUT("/", u.me)
		d.PUT("/picture", u.me)
	})
	d.GET("/:id/", u.getUser)
}

func (u *user) me(c2 echo.Context) error {
	c := c2.(*models.Context)
	return c.JSON(200, c.User)
}

func (u *user) getUser(c echo.Context) error {
	id, err := strconv.Atoi(c.Param("id"))
	if err != nil {
		return err
	}
	out := models.User{}
	err = u.DB.Q().Where("id = ?", id).First(&out)
	if err != nil {
		return err
	}
	return c.JSON(200, out)
}

func (u *user) putMe(c2 echo.Context) error {
	c := c2.(*models.Context)
	var item models.User
	err := c.Bind(&item)
	if err != nil {
		return err
	}
	item.ID = c.User.ID
	err = u.DB.Update(&item, "created_at", "permission", "picture")
	if err != nil {
		return err
	}
	return c.NoContent(200)
}

func (u *user) putMePicture(c2 echo.Context) error {
	c := c2.(*models.Context)
	yes, err := u.Pic.IsCool(c.User.ID)
	if err != nil {
		return err
	}
	if yes {
		return echo.NewHTTPError(403, "Try again later")
	}

	defer c.Request().Body.Close()
	buf, err := ioutil.ReadAll(c.Request().Body)
	if err != nil {
		return err
	}

	picURL, err := u.Pic.UploadBuffer(buf)
	if err != nil {
		return err
	}

	err = u.Pic.SetCool(c.User.ID)
	if err != nil {
		return err
	}

	c.User.Picture = nulls.NewString(picURL)
	err = u.DB.Update(&c.User)
	if err != nil {
		return err
	}

	return c.NoContent(200)
}
