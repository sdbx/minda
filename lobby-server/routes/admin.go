package routes

import (
	"lobby/middlewares"
	"github.com/sunho/dim"
)

type admin struct {
}

func (a *admin) Register(g *dim.Group) {
	g.Use(&middlewares.AuthMiddleware{})
	g.Use(&middlewares.AdminMiddleware{})
	g.Route("/users", &adminUser{})
	g.Route("/gameservers", &gameServer{})
}
