package middlewares

import (
	"lobby/models"

	"github.com/labstack/echo"
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
