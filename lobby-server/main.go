package main

import (
	"lobby/routes"
	"lobby/servs/gameserv"
	"lobby/servs/redisserv"
	"lobby/servs/dbserv"
	"lobby/servs/oauthserv"
	"lobby/servs/taskserv"

	"github.com/sunho/dim"
)

func main() {
	d := dim.New()
	d.Provide(oauthserv.Provide)
	d.Provide(redisserv.Provide)
	d.Provide(gameserv.Provide)
	d.Provide(taskserv.Provide)
	d.Provide(dbserv.Provide)
	d.Init("")
	d.Register(routes.Register)
	d.Start(":8080")
}
