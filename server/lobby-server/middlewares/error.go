package middlewares

import (
	"lobby/servs/oauthserv"
	"lobby/utils"
	"strconv"
	"strings"

	"go.uber.org/zap"

	"github.com/labstack/echo"
)

func ErrorMiddleware(next echo.HandlerFunc) echo.HandlerFunc {
	return func(c echo.Context) error {
		err := next(c)
		if err == nil {
			return nil
		}
		utils.Log.Error("Error in http handler", zap.Error(err))
		switch err.(type) {
		case *echo.HTTPError:
			return err
		case *strconv.NumError:
			return echo.NewHTTPError(400, "Bad request")
		default:
			switch err {
			case oauthserv.ErrNotAuthorized:
				return echo.NewHTTPError(403, "Incomplete auth request")
			case utils.ErrNoGameServer:
				return echo.NewHTTPError(500, "No gameserver available")
			case utils.ErrNotExists:
				return echo.NewHTTPError(404, "No such resource")
			default:
				if strings.Contains(err.Error(), "no rows in result") {
					return echo.NewHTTPError(404, "No such resource")
				}
				return echo.NewHTTPError(500)
			}
		}
	}
}
