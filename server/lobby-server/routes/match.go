package routes

import (
	"lobby/middlewares"
	"lobby/models"
	"lobby/servs/matchserv"

	"github.com/labstack/echo"
	"github.com/sunho/dim"
)

type match struct {
	Match *matchserv.MatchServ `dim:"on"`
}

func (m *match) Register(g *dim.Group) {
	g.Use(&middlewares.AuthMiddleware{})
	g.GET("/", m.Get)
	g.DELETE("/", m.Delete)
}

func (m *match) Get(c *models.Context) error {
	
	id, err := m.Match.FindMatch(c.User.ID)
	if err == matchserv.ErrNotFound {
		return echo.NewHTTPError(404, "not found yet")
	}
	if err != nil {
		return err
	}
	out := struct {
		RoomID string `json:"room_id"`
	}{
		RoomID: id,
	}
	return c.JSON(200, out)
}

func (m *match) Delete(c *models.Context) error {
	
	m.Match.CancelSearching(c.User.ID)
	return c.NoContent(200)
}
