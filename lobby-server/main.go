package main

import (
	"lobby/serv/redisserv"
	"lobby/routes"
	"lobby/serv/dbserv"
	"lobby/serv/oauthserv"
	"lobby/serv/taskserv"

	"github.com/sunho/dim"
)

func main() {
	d := dim.New()
	d.Provide(oauthserv.Provide)
	d.Provide(redisserv.Provide)
	d.Provide(taskserv.Provide)
	d.Provide(dbserv.Provide)
	d.Init("")
	d.Register(routes.Register)
	d.Start(":8080")
}
