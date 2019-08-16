package middlewares

import (
	"github.com/labstack/echo"
	"lobby/models"
)

type AdminMiddleware struct {
}

func (p *AdminMiddleware) Act(c2 echo.Context) error {
	c := c2.(*models.Context)
	if !c.User.Permission.Admin {
		return echo.NewHTTPError(403, "Not enough permission")
	}
	return nil
}