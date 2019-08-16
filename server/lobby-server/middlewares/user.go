package middlewares

import (
	"lobby/models"
	"lobby/servs/authserv"
	"strconv"

	"github.com/labstack/echo"
)

type UserMiddleware struct {
	Auth *authserv.AuthServ `dim:"on"`
}

func (u *UserMiddleware) Act(c2 echo.Context) error {
	c := c2.(*models.Context)
	tmp := c.Param("userid")
	id, err := strconv.Atoi(tmp)
	if err != nil {
		return err
	}

	user, err := u.Auth.GetUser(id)
	if err != nil {
		return err
	}

	c.UserParam = user
	return nil
}
