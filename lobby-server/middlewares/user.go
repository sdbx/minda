package middlewares

import (
	"lobby/models"
	"lobby/servs/authserv"

	"github.com/labstack/echo"
)

type UserMiddleware struct {
	Auth *authserv.AuthServ `dim:"on"`
}

func (a *UserMiddleware) Act(c echo.Context) error {
	token := c.Request().Header.Get("Authorization")
	if token == "" {
		return echo.NewHTTPError(400, "No authorization header")
	}
	user, err := a.Auth.Authorize(token)
	if err != nil {
		return echo.NewHTTPError(403, "Invalid token")
	}
	c2 := c.(*models.Context)
	c2.User = user
	return nil
}
