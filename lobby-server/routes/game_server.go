package routes

import (
	"lobby/servs/discserv"

	"github.com/labstack/echo"
	"github.com/sunho/dim"
)

type gameServer struct {
	Disc *discserv.DiscoverServ `dim:"on"`
}

func (g *gameServer) Register(d *dim.Group) {
	d.GET("/", g.getGameServers)
}

func (g *gameServer) getGameServers(c echo.Context) error {
	servers, err := g.Disc.ListGameServers()
	if err != nil {
		return err
	}
	return c.JSON(200, servers)
}
