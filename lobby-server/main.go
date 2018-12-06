package main

import (
	"lobby/routes"
	"lobby/serv/dbserv"
	"lobby/serv/authserv"
	"lobby/serv/taskserv"

	"github.com/sunho/dim"
)

func main() {
	d := dim.New()
	d.Provide(authserv.Provide)
	d.Provide(taskserv.Provide)
	d.Provide(dbserv.Provide)
	d.Init("")
	d.Register(routes.Register)
	d.Start(":8080")
}
