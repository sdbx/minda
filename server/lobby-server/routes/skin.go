package routes

import (
	"bytes"
	"image/png"
	"lobby/middlewares"
	"lobby/models"
	"lobby/servs/authserv"
	"lobby/servs/dbserv"
	"lobby/servs/payserv"
	"lobby/servs/picserv"
	"lobby/utils"
	"mime/multipart"
	"strconv"

	"github.com/gobuffalo/pop/nulls"
	"github.com/labstack/echo"
	"github.com/sunho/dim"
)

type skin struct {
	Auth *authserv.AuthServ `dim:"on"`
	DB   *dbserv.DBServ     `dim:"on"`
	Pay  *payserv.PayServ   `dim:"on"`
	Pic  *picserv.PicServ   `dim:"on"`
}

func (s *skin) Register(d *dim.Group) {
	d.Use(&middlewares.AuthMiddleware{})
	d.POST("/preview/", s.previewSkin)
	d.POST("/buy/", s.postBuy)
	d.PUT("/buy/:orderid/", s.putBuy)
	d.RouteFunc("/me", func(d *dim.Group) {
		d.GET("/", s.getSkins)
		d.PUT("/current/", s.putCurrent)
		d.POST("/one/", s.postOneSkin)
		d.POST("/two/", s.postTwoSkin)
	})
	d.GET("/:id/", s.getSkin)
}

func (s *skin) getSkin(c echo.Context) error {
	id, err := strconv.Atoi(c.Param("id"))
	if err != nil {
		return err
	}

	var item models.Skin
	err = s.DB.Q().Where("id = ?", id).First(&item)
	if err != nil {
		return err
	}

	return c.JSON(200, item)
}

func (s *skin) getSkins(c2 echo.Context) error {
	c := c2.(*models.Context)
	out, err := s.DB.GetSkinsOfUser(c.User.ID)
	if err != nil {
		return err
	}
	return c.JSON(200, out)
}

func (s *skin) uploadSkin(user int, name string, black *multipart.FileHeader, white *multipart.FileHeader) error {
	img, err := s.Pic.ParseImageFromFile(black)
	if err != nil {
		return err
	}

	blackURL, err := s.Pic.UploadImage(s.Pic.CreateSkin(img, picserv.ColorBlack))
	if err != nil {
		return err
	}

	img, err = s.Pic.ParseImageFromFile(white)
	if err != nil {
		return err
	}

	whiteURL, err := s.Pic.UploadImage(s.Pic.CreateSkin(img, picserv.ColorWhite))
	if err != nil {
		return err
	}

	item := models.Skin{
		Name:         name,
		BlackPicture: blackURL,
		WhitePicture: whiteURL,
	}
	err = s.DB.Eager().Create(&item)
	if err != nil {
		return err
	}

	return s.DB.Create(&models.UserSkin{
		UserID: user,
		SkinID: item.ID,
	})
}

func (s *skin) putCurrent(c2 echo.Context) error {
	c := c2.(*models.Context)
	input := struct {
		ID *int `json:"id"`
	}{}
	if err := c.Bind(&input); err != nil {
		return err
	}

	id := input.ID
	skins, err := s.DB.GetSkinsOfUser(c.User.ID)
	if err != nil {
		return err
	}
	if id == nil {
		c.User.Inventory.CurrentSkin = nulls.Int{Valid: false}
		err = s.DB.Update(&c.User.Inventory)
		if err != nil {
			return err
		}
		return c.NoContent(200)
	}
	for _, skin := range skins {
		if skin.ID == *id {
			c.User.Inventory.CurrentSkin = nulls.NewInt(*id)
			err = s.DB.Update(&c.User.Inventory)
			if err != nil {
				return err
			}
			return c.NoContent(200)
		}
	}
	return utils.ErrNotExists
}

func (s *skin) postOneSkin(c2 echo.Context) error {
	c := c2.(*models.Context)
	if c.User.Inventory.OneColorSkin <= 0 {
		return echo.NewHTTPError(400, "Not enough point")
	}

	name := c.FormValue("name")
	f, err := c.FormFile("file")
	if err != nil {
		return err
	}

	err = s.uploadSkin(c.User.ID, name, f, f)
	if err != nil {
		return err
	}

	c.User.Inventory.OneColorSkin--
	err = s.DB.Update(&c.User.Inventory)
	if err != nil {
		return err
	}
	return c.NoContent(201)
}

func (s *skin) postTwoSkin(c2 echo.Context) error {
	c := c2.(*models.Context)
	if c.User.Inventory.TwoColorSkin <= 0 {
		return echo.NewHTTPError(400, "Not enough point")
	}

	name := c.FormValue("name")

	f, err := c.FormFile("black")
	if err != nil {
		return err
	}
	f2, err := c.FormFile("white")
	if err != nil {
		return err
	}

	err = s.uploadSkin(c.User.ID, name, f, f2)
	if err != nil {
		return err
	}

	err = s.DB.Create(&models.SkinLog{
		UserID: c.User.ID,
		Dif:    -1,
	})
	if err != nil {
		return err
	}

	c.User.Inventory.TwoColorSkin--
	err = s.DB.Update(&c.User.Inventory)
	if err != nil {
		return err
	}
	return c.NoContent(201)
}

func (s *skin) previewSkin(c echo.Context) error {
	col := picserv.ColorBlack
	if c.FormValue("color") == "white" {
		col = picserv.ColorWhite
	}

	f, err := c.FormFile("file")
	if err != nil {
		return err
	}

	img, err := s.Pic.ParseImageFromFile(f)
	if err != nil {
		return err
	}

	out := s.Pic.CreateSkin(img, col)

	var buf bytes.Buffer
	err = png.Encode(&buf, out)
	if err != nil {
		return err
	}
	return c.Stream(200, "image/png", &buf)
}

func (s *skin) postBuy(c2 echo.Context) error {
	c := c2.(*models.Context)
	err := s.Pay.InitOrder(c.User.ID)
	if err != nil {
		return err
	}
	return c.NoContent(200)
}

func (s *skin) putBuy(c echo.Context) error {
	id, err := strconv.Atoi(c.Param("orderid"))
	if err != nil {
		return err
	}

	err = s.Pay.FinalizeOrder(id)
	if err != nil {
		return err
	}

	return nil
}
