package middlewares

import (
	"github.com/golang/glog"
	"github.com/labstack/echo"
)

func ErrorMiddleware(next echo.HandlerFunc) echo.HandlerFunc {
	return func(c echo.Context) error {
		err := next(c)
		if err == nil {
			return nil
		}
		glog.Errorf("Error in http handler %v", err)
		return echo.NewHTTPError(400)
	}
}
