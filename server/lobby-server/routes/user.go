package routes

import (
	"lobby/middlewares"
	"lobby/models"
	"lobby/servs/dbserv"
	"lobby/servs/picserv"
	"strconv"

	"github.com/gobuffalo/pop/nulls"

	"github.com/labstack/echo"
	"github.com/sunho/dim"
)

type user struct {
	DB  *dbserv.DBServ   `dim:"on"`
	Pic *picserv.PicServ `dim:"on"`
}

func (u *user) Register(d *dim.Group) {
	d.RouteFunc("/me", func(d *dim.Group) {
		d.Use(&middlewares.AuthMiddleware{})
		d.GET("/", u.me)
		d.PUT("/", u.putMe)
		d.PUT("/picture/", u.putMePicture)
	})
	d.GET("/:id/", u.getUser)
}

func (u *user) me(c *models.Context) error {
	return c.JSON(200, c.User)
}

func (u *user) getUser(c *models.Context) error {
	id, err := strconv.Atoi(c.Param("id"))
	if err != nil {
		return err
	}
	out := models.User{}
	err = u.DB.Eager().Q().Where("id = ?", id).First(&out)
	if err != nil {
		return err
	}
	return c.JSON(200, out)
}

func (u *user) putMe(c *models.Context) error {
	var item models.User
	err := c.Bind(&item)
	if err != nil {
		return err
	}
	item.ID = c.User.ID
	err = u.DB.Update(&item, "created_at", "picture", "user_permission", "user_inventory")
	if err != nil {
		return err
	}
	return c.NoContent(200)
}

func (u *user) putMePicture(c *models.Context) error {
	yes, err := u.Pic.IsCool(c.User.ID)
	if err != nil {
		return err
	}
	if yes {
		return echo.NewHTTPError(403, "Try again later")
	}

	file, err := c.FormFile("file")
	if err != nil {
		return err
	}

	img, err := u.Pic.ParseImageFromFile(file)
	if err != nil {
		return err
	}

	picURL, err := u.Pic.UploadImage(u.Pic.CreateProfile(img))
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
