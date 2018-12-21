package main

import (
	"time"
	"math/rand"
	"lobby/routes"
	"lobby/servs/authserv"
	"lobby/servs/discserv"
	"lobby/servs/redisserv"
	"lobby/servs/taskserv"

	"github.com/sunho/dim"
)

func main() {
	rand.Seed(time.Now().Unix())
	d := dim.New()
	d.Provide(authserv.Provide, redisserv.Provide, discserv.Provide, taskserv.Provide)
	d.Init("")
	d.Register(routes.Register)
	d.Start(":8080")
}
