package main

import (
	"lobby/routes"
	"lobby/servs/authserv"
	"lobby/servs/dbserv"
	"lobby/servs/discserv"
	"lobby/servs/matchserv"
	"lobby/servs/oauthserv"
	"lobby/servs/payserv"
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
	d.Provide(matchserv.Provide, payserv.Provide, dbserv.Provide, authserv.Provide, redisserv.Provide, discserv.Provide, taskserv.Provide, oauthserv.Provide, picserv.Provide, steamserv.Provide)
	d.Init("", true)
	d.Register(routes.Register)
	d.Start(":8080")
}
