package routes

import (
	"lobby/models"
	"lobby/servs/dbserv"
	"strconv"
	"time"

	"github.com/labstack/echo"
	"github.com/sunho/dim"
)

type history struct {
	DB *dbserv.DBServ `dim:"on"`
}

func (h *history) Register(d *dim.Group) {
	d.GET("/", h.getHistories)
}

func (h *history) getHistories(c echo.Context) error {
	q := h.DB.Eager().Q()
	user2 := c.QueryParam("user")
	if user2 != "" {
		user, err := strconv.Atoi(user2)
		if err != nil {
			return err
		}
		q = q.Where("black = ? OR white = ?", user, user)
	}

	page2 := c.QueryParam("p")
	if page2 == "" {
		page2 = "0"
	}
	page, err := strconv.Atoi(page2)
	if err != nil {
		return err
	}
	q = q.Paginate(page, 10)

	since2 := c.QueryParam("since")
	if since2 != "" {
		since, err := time.Parse(time.RFC3339, since2)
		if err != nil {
			return err
		}
		q = q.Where("created_at > ?", since)
	}

	var out []models.History
	err = q.All(&out)
	if err != nil {
		return err
	}

	return c.JSON(200, out)
}
