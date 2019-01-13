package routes

import (
	"lobby/utils"
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
	d.Use(&middlewares.AuthMiddleware{})
	d.GET("/", r.listRoom)
	d.POST("/", r.postRoom)
	d.PUT("/:roomid/", r.putRoom)
}

func (r *room) listRoom(c echo.Context) error {
	return c.JSONPretty(200, r.Disc.GetRooms(), "\t")
}

func (r *room) postRoom(c2 echo.Context) error {
	c := c2.(*models.Context)

	user := c.User
	conf := models.RoomConf{}
	err := c.Bind(&conf)
	if err != nil {
		return err
	}

	rooms, err := r.Disc.FetchRooms(true)
	if err != nil {
		return err
	}

	for _, room := range rooms {
		if room.Conf.King == user.ID {
			if len(room.Users) == 0 {
				_, err := r.Task.Request(room.Server, &models.DeleteRoomTask{
					RoomID: room.ID,
				})
				if err != nil {
					return err
				}
			} else { 
				_, err = r.Task.Request(room.Server, &models.KickUserTask{
					UserID: user.ID,
					RoomID: room.ID,
				})
				if err != nil {
					return err
				}
			}
		}
	}
	
	servers, err := r.Disc.ListGameServers()
	if err != nil {
		return err
	}
	if len(servers) == 0 {
		return utils.ErrNoGameServer
	}

	conf.White = -1
	conf.Black = -1
	conf.King = user.ID
	res, err := r.Task.Request(servers[rand.Intn(len(servers))].Name, &models.CreateRoomTask{
		Conf: conf,
		UserID: user.ID,
	})
	if err != nil {
		return err
	}

	return c.JSONPretty(201, res.(*models.LobbyRoomResult), "\t")
}

func (r *room) putRoom(c2 echo.Context) error {
	c := c2.(*models.Context)
	id := c.Param("roomid")
	rooms, err := r.Disc.FetchRooms(false)
	if err != nil {
		return err
	}
	for _, room := range rooms {
		if room.ID == id {
			user := c.User
			res, err := r.Task.Request(room.Server, &models.JoinRoomTask{
				UserID: user.ID,
				RoomID: id,
			})
			if err != nil {
				return err
			}
			return c.JSONPretty(200, res.(*models.LobbyRoomResult), "\t")
		}
	}
	return utils.ErrNotExists
}
