package routes

import (
	"lobby/middlewares"
	"lobby/models"
	"lobby/servs/dbserv"
	"strconv"

	"github.com/labstack/echo"
	"github.com/sunho/dim"
)

type mapr struct {
	DB *dbserv.DBServ `dim:"on"`
}

func (m *mapr) Register(d *dim.Group) {
	d.Use(&middlewares.AuthMiddleware{})
	d.GET("/", m.getMaps)
	d.POST("/", m.postMap)
	d.DELETE("/:mapid/", m.deleteMap)
}

func (m *mapr) getMaps(c2 echo.Context) error {
	c := c2.(*models.Context)
	var out []models.Map
	err := m.DB.Q().
		InnerJoin("user_maps", "maps.id = user_maps.map_id").
		Where("user_maps.user_id = ?", c.User.ID).All(&out)
	if err != nil {
		return err
	}
	return c.JSON(200, out)
}

func (m *mapr) postMap(c2 echo.Context) error {
	c := c2.(*models.Context)
	var item models.Map
	err := c.Bind(&item)
	if err != nil {
		return err
	}

	_, err = item.Payload.Parse()
	if err != nil {
		return err
	}

	item.ID = 0
	err = m.DB.Create(&item)
	if err != nil {
		return err
	}
	err = m.DB.Create(&models.UserMap{
		UserID: c.User.ID,
		MapID:  item.ID,
	})
	if err != nil {
		return err
	}

	return c.NoContent(201)
}

func (m *mapr) deleteMap(c2 echo.Context) error {
	c := c2.(*models.Context)
	tmp := c.Param("mapid")
	mapid, err := strconv.Atoi(tmp)
	if err != nil {
		return err
	}

	var item models.UserMap
	err = m.DB.Q().
		Where("user_id = ? AND map_id = ?", c.User.ID, mapid).
		First(&item)
	if err != nil {
		return err
	}

	err = m.DB.Destroy(&item)
	if err != nil {
		return err
	}

	return c.NoContent(200)
}
