package routes

import (
	"lobby/middlewares"
	"lobby/models"

	"github.com/sunho/dim"
)

func HelloWorld(c *models.Context) error {
	return c.String(200, "Hello, World!")
}

func Register(d *dim.Group) {
	d.Group.Use(middlewares.ErrorMiddleware)
	d.Group.Use(middlewares.ContextMiddleware)
	d.GET("/", HelloWorld)
	d.Route("/users", &user{})
	d.Route("/rooms", &room{})
	d.Route("/auth", &auth{})
	d.Route("/admin", &admin{})
	d.Route("/maps", &mapr{})
	d.Route("/skins", &skin{})
	d.Route("/match", &match{})
	d.Route("/histories", &history{})
}
