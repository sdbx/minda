package routes

import (
	"lobby/models"
	"lobby/servs/discserv"

	"github.com/sunho/dim"
)

type gameServer struct {
	Disc *discserv.DiscoverServ `dim:"on"`
}

func (g *gameServer) Register(d *dim.Group) {
	d.GET("/", g.getGameServers)
}

func (g *gameServer) getGameServers(c *models.Context) error {
	servers, err := g.Disc.ListGameServers()
	if err != nil {
		return err
	}
	return c.JSONPretty(200, servers, "\t")
}
