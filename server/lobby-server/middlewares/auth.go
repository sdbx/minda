package middlewares

import (
	"lobby/models"
	"lobby/servs/authserv"

	"github.com/labstack/echo"
)

type AuthMiddleware struct {
	Auth *authserv.AuthServ `dim:"on"`
}

func (a *AuthMiddleware) Act(c2 echo.Context) error {
	c := c2.(*models.Context)
	token := c.Request().Header.Get("Authorization")
	if token == "" {
		return echo.NewHTTPError(400, "No authorization header")
	}
	user, err := a.Auth.Authorize(token)
	if err != nil {
		return echo.NewHTTPError(403, "Invalid token")
	}
	c.User = user
	return nil
}
