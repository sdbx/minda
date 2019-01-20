package models

import (
	"database/sql/driver"
	"errors"
	"fmt"
	"time"

	"github.com/gobuffalo/uuid"

	"github.com/gobuffalo/pop/nulls"
)

type User struct {
	ID         int            `db:"id" json:"id"`
	Username   string         `db:"username" json:"username"`
	Picture    nulls.Int      `db:"picture" json:"picture"`
	CreatedAt  time.Time      `db:"created_at" json:"-"`
	UpdatedAt  time.Time      `db:"updated_at" json:"-"`
	Permission UserPermission `has_one:"user_permission" json:"permission"`
}

type UserPermission struct {
	ID     uuid.UUID `db:"id" json:"-"`
	UserID int       `db:"user_id" json:"-"`
	Admin  bool      `db:"admin" json:"admin"`
}

type OAuthUser struct {
	Provider  string    `db:"provider"`
	ID        string    `db:"id"`
	UserID    int       `db:"user_id"`
	CreatedAt time.Time `db:"created_at"`
	UpdatedAt time.Time `db:"updated_at"`
}

func (o OAuthUser) TableName() string {
	return "oauth_users"
}

type AuthRequest struct {
	Token *string `json:"token"`
	First bool    `json:"first"`
}

type History struct {
	ID        int           `db:"id"`
	CreatedAt time.Time     `db:"created_at"`
	UpdatedAt time.Time     `db:"updated_at"`
	Black     int           `db:"black"`
	White     int           `db:"white"`
	Map       string        `db:"map"`
	Moves     []HistoryMove `db:"moves" has_many:"history_moves" order_by:"seq asc"`
}

type HistoryMove struct {
	ID        int  `db:"id" json:"-"`
	HistoryID int  `db:"history_id" json:"-"`
	Player    int  `db:"player" json:"player"`
	From      Cord `db:"from" json:"from"`
	To        Cord `db:"to" json:"to"`
}

type Cord struct {
	X int `json:"x"`
	Y int `json:"y"`
	Z int `json:"z"`
}

func (c Cord) Value() (driver.Value, error) {
	return driver.Value(fmt.Sprintf("%d-%d-%d", c.X, c.Y, c.Z)), nil
}

func (c *Cord) Scan(i interface{}) error {
	var src string
	switch i.(type) {
	case string:
		src = i.(string)
	case []byte:
		src = string(i.([]byte))
	default:
		return errors.New("Incompatible type for Cord")
	}
	_, err := fmt.Sscanf("%d-%d-%d", src, &c.X, &c.Y, &c.Z)
	return err
}
