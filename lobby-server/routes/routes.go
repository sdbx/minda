package routes

import (
	"github.com/labstack/echo"
	"github.com/sunho/dim"
)

func HelloWorld(c echo.Context) error {
	return c.String(200, "Hello, World!")
}

func Register(d *dim.Group) {
	d.GET("/", HelloWorld)
	d.Route("/gameservers", &gameServer{})
	d.Route("/rooms", &room{})
}
