package routes

import (
	"lobby/utils"
	"github.com/gobuffalo/pop/nulls"
	"strconv"
	"lobby/middlewares"
	"image/png"
	"bytes"
	"mime/multipart"
	"lobby/servs/picserv"
	"lobby/models"
	"github.com/labstack/echo"
	"github.com/sunho/dim"
	"lobby/servs/dbserv"
)

type skin struct {
	DB *dbserv.DBServ `dim:"on"`
	Pic *picserv.PicServ `dim:"on"`
}

func (s *skin) Register(d *dim.Group) {
	d.Use(&middlewares.AuthMiddleware{})
	d.POST("/preview/", s.previewSkin)
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

	item := models.Skin {
		Name: name,
		BlackPicture: blackURL,
		WhitePicture: whiteURL,
	}
	err = s.DB.Eager().Create(&item)
	if err != nil {
		return err
	}

	return s.DB.Create(&models.UserSkin {
		UserID: user,
		SkinID: item.ID,
	})
}

func (s *skin) putCurrent(c2 echo.Context) error {
	c := c2.(*models.Context)
	input := struct {
		ID int `json:"id"`	
	}{}
	if err := c.Bind(&input); err != nil {
		return err
	}

	id := input.ID
	skins, err := s.DB.GetSkinsOfUser(c.User.ID)
	if err != nil {
		return err
	}
	for _, skin := range skins {
		if skin.ID == id {
			c.User.Inventory.CurrentSkin = nulls.NewInt(id)
			err = s.DB.Eager().Save(&c.User)
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

	c.User.Inventory.OneColorSkin -- 
	err = s.DB.Update(&c.User)
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

	c.User.Inventory.TwoColorSkin -- 
	err = s.DB.Update(&c.User)
	if err != nil {
		return err
	}
	return c.NoContent(201)
}

func (s *skin) previewSkin(c echo.Context) error {
	col := picserv.ColorBlack
	if c.FormValue("color")== "white" {
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
