package routes

import (
	"lobby/middlewares"

	"github.com/labstack/echo"
	"github.com/sunho/dim"
)

func HelloWorld(c echo.Context) error {
	return c.String(200, "Hello, World!")
}

func Register(d *dim.Group) {
	d.UseRaw(middlewares.ErrorMiddleware)
	d.UseRaw(middlewares.ContextMiddleware)
	d.GET("/", HelloWorld)
	d.Route("/users", &user{})
	d.Route("/rooms", &room{})
	d.Route("/auth", &auth{})
	d.Route("/admin", &admin{})
	d.Route("/maps", &mapr{})
	d.Route("/skins", &skin{})
	d.Route("/histories", &history{})
}
