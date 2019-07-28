package routes

import (
	"lobby/middlewares"
	"lobby/models"
	"lobby/servs/discserv"
	"lobby/servs/taskserv"
	"lobby/utils"
	"math/rand"
	"net/http"
	"strings"

	"github.com/labstack/echo"
	"github.com/sunho/dim"
)

type room struct {
	Task *taskserv.TaskServ     `dim:"on"`
	Disc *discserv.DiscoverServ `dim:"on"`
}

func (r *room) Register(d *dim.Group) {
	d.GET("/", r.listRoom)
	d.RouteFunc("/", func(d *dim.Group) {
		d.Use(&middlewares.AuthMiddleware{})
		d.POST("", r.postRoom)
		d.PUT(":roomid/", r.putRoom)
	})
}

func (r *room) listRoom(c echo.Context) error {
	rooms := r.Disc.GetRooms()
	if name := c.QueryParam("name"); name != "" {
		out := []models.Room{}
		for _, room := range rooms {
			if strings.Contains(room.Conf.Name, name) {
				out = append(out, room)
			}
		}
		return c.JSONPretty(200, out, "\t")
	}
	return c.JSONPretty(200, rooms, "\t")
}

func (r *room) postRoom(c2 echo.Context) error {
	c := c2.(*models.Context)

	user := c.User
	conf := models.RoomConf{}
	err := c.Bind(&conf)
	if err != nil {
		return err
	}

	rooms, err := r.Disc.FetchRooms(true, true)
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
	if conf.Map == "" {
		conf.Map = "0@0@0@0@0@0@0@1@1#0@0@0@0@0@0@0@1@1#0@0@0@0@0@0@1@1@1#0@2@0@0@0@0@1@1@1#2@2@2@0@0@0@1@1@1#2@2@2@0@0@0@0@1@0#2@2@2@0@0@0@0@0@0#2@2@0@0@0@0@0@0@0#2@2@0@0@0@0@0@0@0"
	}
	if conf.GameRule.GameTimeout == 0 {
		conf.GameRule.GameTimeout = 600
	}
	if conf.GameRule.TurnTimeout == 0 {
		conf.GameRule.TurnTimeout = 20
	}
	if conf.GameRule.DefeatLostStones == 0 {
		conf.GameRule.DefeatLostStones = 6
	}
	if !conf.Validate() {
		return echo.NewHTTPError(400, http.StatusBadRequest)
	}

	id, err := r.Disc.CreateRoomID()
	if err != nil {
		return err
	}
	res, err := r.Task.Request(servers[rand.Intn(len(servers))].Name, &models.CreateRoomTask{
		RoomID: id,
		Conf:   conf,
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
	rooms, err := r.Disc.FetchRooms(false, true)
	if err != nil {
		return err
	}
	user := c.User
	for _, room := range rooms {
		if room.ID == id {
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
