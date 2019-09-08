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
	"log"
	"math/rand"
	"net/http"
	"os"
	"time"

	"github.com/labstack/echo/middleware"
	"github.com/sunho/dim"
)

func main() {
	rand.Seed(time.Now().Unix())
	log.SetOutput(os.Stdout)
	d := dim.New()
	d.E.Use(middleware.CORSWithConfig(middleware.CORSConfig{
		AllowOrigins: []string{"*"},
		AllowMethods: []string{http.MethodGet, http.MethodPut, http.MethodPost, http.MethodDelete},
	}))
	d.Provide(matchserv.Provide, payserv.Provide, dbserv.Provide, authserv.Provide, redisserv.Provide, discserv.Provide, taskserv.Provide, oauthserv.Provide, picserv.Provide, steamserv.Provide)
	err := d.Init("", true)
	if err != nil {
		log.Println(err)
		return
	}
	d.Register(routes.Register)
	d.Start(":8080")
}
