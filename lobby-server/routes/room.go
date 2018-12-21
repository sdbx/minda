package routes

import (
	"lobby/middlewares"
	"math/rand"
	"lobby/servs/taskserv"
	"lobby/models"
	"lobby/servs/discserv"

	"github.com/labstack/echo"
	"github.com/sunho/dim"
)

type room struct {
	Task *taskserv.TaskServ `dim:"on"`
	Disc *discserv.DiscoverServ `dim:"on"`
}

func (r *room) Register(d *dim.Group) {
	d.Use(&middlewares.UserMiddleware{})
	d.GET("/", r.listRoom)
	d.POST("/", r.postRoom)
}

func (r *room) listRoom(c echo.Context) error {
	return c.JSONPretty(200, r.Disc.GetRooms(), "\t")
}

func (r *room) postRoom(c2 echo.Context) error {
	c := c2.(*models.Context)

	room := models.Room{}
	err := c.Bind(&room)
	if err != nil {
		return err
	}

	servers, err := r.Disc.ListGameServers()
	if err != nil {
		return err
	}

	if len(servers) == 0 {
		return echo.NewHTTPError(503, "no game server available")
	}

	res, err := r.Task.Request(servers[rand.Intn(len(servers))].Name, &models.CreateRoomTask{
		Name: room.Name,
		User: *c.User,
	})
	if err != nil {
		return err
	}

	return c.String(201, res.(*models.CreateRoomResult).ID)
}
