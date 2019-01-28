package models

import (
	"database/sql/driver"
	"errors"
	"fmt"
	"time"
)

type History struct {
	ID        int       `db:"id" json:"id"`
	CreatedAt time.Time `db:"created_at" json:"created_at"`
	UpdatedAt time.Time `db:"updated_at" json:"updated_at"`
	Black     int       `db:"black" json:"black"`
	White     int       `db:"white" json:"white"`
	Map       string    `db:"map" json:"map"`
	Moves     []Move    `json:"moves" has_many:"moves" order_by:"seq asc"`
}

func (h History) TableName() string {
	return "histories"
}

type Move struct {
	ID        int  `db:"id" json:"-"`
	HistoryID int  `db:"history_id" json:"-"`
	Player    int  `db:"player" json:"player"`
	Start     Cord `db:"start_cord" json:"start"`
	End       Cord `db:"end_cord" json:"end"`
	Dir       Cord `db:"dir_cord" json:"dir"`
}

type Cord struct {
	X int `json:"x"`
	Y int `json:"y"`
	Z int `json:"z"`
}

func (c Cord) Value() (driver.Value, error) {
	return driver.Value(fmt.Sprintf("%d=%d=%d", c.X, c.Y, c.Z)), nil
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
	_, err := fmt.Sscanf("%d=%d=%d", src, &c.X, &c.Y, &c.Z)
	return err
}
