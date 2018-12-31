package middlewares

import (
	"lobby/servs/oauthserv"
	"lobby/utils"

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
		switch err.(type) {
		case *echo.HTTPError:
			return err
		default:
			switch err {
			case oauthserv.ErrNotAuthorized:
				return echo.NewHTTPError(403, "Incomplete auth request")
			case utils.ErrNoGameServer:
				return echo.NewHTTPError(500, "No gameserver available")
			case utils.ErrNotExists:
				return echo.NewHTTPError(404, "No such resource")
			default:
				return echo.NewHTTPError(500)
			}
		}
	}
}
