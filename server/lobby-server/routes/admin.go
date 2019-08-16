package routes

import (
	"lobby/middlewares"

	"github.com/sunho/dim"
)

type admin struct {
}

func (a *admin) Register(d *dim.Group) {
	d.Use(&middlewares.AuthMiddleware{})
	d.Use(&middlewares.AdminMiddleware{})
	d.Route("/users", &adminUser{})
	d.Route("/gameservers", &gameServer{})
}
