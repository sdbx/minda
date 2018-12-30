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
	d.PUT("/:roomid/", r.putRoom)
}

func (r *room) listRoom(c echo.Context) error {
	return c.JSONPretty(200, r.Disc.GetRooms(), "\t")
}

func (r *room) postRoom(c2 echo.Context) error {
	c := c2.(*models.Context)

	conf := models.RoomConf{}
	err := c.Bind(&conf)
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

	user := *c.User
	conf.King = user.ID
	res, err := r.Task.Request(servers[rand.Intn(len(servers))].Name, &models.CreateRoomTask{
		Conf: conf,
		User: user.ID,
	})
	if err != nil {
		return err
	}

	return c.JSONPretty(201, res.(*models.CreateRoomResult), "\t")
}

func (r *room) putRoom(c2 echo.Context) error {
	c := c2.(*models.Context)
	id := c.Param("roomid")
	rooms := r.Disc.GetRooms()
	for _, room := range rooms {
		if room.ID == id {
			user := *c.User
			res, err := r.Task.Request(room.Server, &models.JoinRoomTask{
				User: user.ID,
				Room: id,
			})
			if err != nil {
				return err
			}
			return c.JSONPretty(200, res.(*models.JoinRoomResult), "\t")
		}
	}
	return c.String(404, "no such room")
}
