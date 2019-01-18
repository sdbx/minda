package routes

import (
	"io/ioutil"
	"lobby/middlewares"
	"lobby/models"
	"lobby/servs/picserv"
	"strconv"

	"github.com/labstack/echo"
	"github.com/sunho/dim"
)

type pic struct {
	Pic *picserv.PicServ `dim:"on"`
}

func (p *pic) Register(d *dim.Group) {
	d.GET("/:picid/", p.getPicture)
	d.RouteFunc("/", func(d *dim.Group) {
		d.Use(&middlewares.AuthMiddleware{})
		d.POST("", p.postPicture)
	})
}

func (p *pic) getPicture(c echo.Context) error {
	tmp := c.Param("picid")
	id, err := strconv.Atoi(tmp)
	if err != nil {
		return err
	}
	pic, err := p.Pic.GetImage(id)
	if err != nil {
		return err
	}
	return c.Blob(200, "image/png", pic)
}

func (p *pic) postPicture(c2 echo.Context) error {
	c := c2.(*models.Context)
	yes, err := p.Pic.IsCool(c.User.ID)
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

	id, err := p.Pic.UploadBuffer(buf)
	if err != nil {
		return err
	}

	err = p.Pic.SetCool(c.User.ID)
	if err != nil {
		return err
	}
	return c.JSON(201, struct {
		PicID int `json:"pic_id"`
	}{
		PicID: id,
	})
}
