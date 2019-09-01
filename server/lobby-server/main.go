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
	"net/http"
	"time"

	"github.com/labstack/echo/middleware"
	"github.com/sunho/dim"
)

func main() {
	rand.Seed(time.Now().Unix())
	d := dim.New()
	d.E.Use(middleware.CORSWithConfig(middleware.CORSConfig{
		AllowOrigins: []string{"*"},
		AllowMethods: []string{http.MethodGet, http.MethodPut, http.MethodPost, http.MethodDelete},
	}))
	d.Provide(matchserv.Provide, payserv.Provide, dbserv.Provide, authserv.Provide, redisserv.Provide, discserv.Provide, taskserv.Provide, oauthserv.Provide, picserv.Provide, steamserv.Provide)
	d.Init("", true)
	d.Register(routes.Register)
	d.Start(":8080")
}
