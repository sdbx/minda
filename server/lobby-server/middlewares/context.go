package middlewares

import (
	"lobby/models"

	"github.com/labstack/echo"
)

func ContextMiddleware(next echo.HandlerFunc) echo.HandlerFunc {
	return func(c echo.Context) error {
		return next(&models.Context{
			Context: c,
		})
	}
}
