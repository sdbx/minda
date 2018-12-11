package main

import (
	"lobby/routes"
	"lobby/servs/discserv"
	"lobby/servs/redisserv"
	"lobby/servs/taskserv"

	"github.com/sunho/dim"
)

func main() {
	d := dim.New()
	d.Provide(redisserv.Provide)
	d.Provide(discserv.Provide)
	d.Provide(taskserv.Provide)
	err := d.Init("")
	if err != nil {
		panic(err)
	}
	d.Register(routes.Register)
	d.Start(":8080")
}
