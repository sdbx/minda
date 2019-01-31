package main

import (
	"lobby/routes"
	"lobby/servs/authserv"
	"lobby/servs/dbserv"
	"lobby/servs/discserv"
	"lobby/servs/oauthserv"
	"lobby/servs/picserv"
	"lobby/servs/redisserv"
	"lobby/servs/steamserv"
	"lobby/servs/taskserv"
	"math/rand"
	"time"

	"github.com/sunho/dim"
)

func main() {
	rand.Seed(time.Now().Unix())
	d := dim.New()
	d.Provide(authserv.Provide, redisserv.Provide, discserv.Provide, taskserv.Provide, oauthserv.Provide, dbserv.Provide, picserv.Provide, steamserv.Provide)
	d.Init("")
	d.Register(routes.Register)
	d.Start(":8080")
}
