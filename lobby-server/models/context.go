package models

import "github.com/labstack/echo"

type Context struct {
	echo.Context
	User User
}
