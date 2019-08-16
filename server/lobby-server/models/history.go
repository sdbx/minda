package models

import (
	"database/sql/driver"
	"errors"
	"fmt"
	"time"

	"github.com/gobuffalo/uuid"
)

type History struct {
	ID        int             `db:"id" json:"id"`
	CreatedAt time.Time       `db:"created_at" json:"created_at"`
	UpdatedAt time.Time       `db:"updated_at" json:"updated_at"`
	Black     int             `db:"black" json:"black"`
	White     int             `db:"white" json:"white"`
	Loser     int             `db:"loser" json:"loser"`
	Cause     string          `db:"cause" json:"cause"`
	Rank      bool            `db:"ranked" json:"rank"`
	Map       string          `db:"map" json:"map"`
	Moves     []Move          `json:"moves" has_many:"moves" order_by:"id asc"`
	Rule      HistoryGameRule `json:"rule" has_one:"history_game_rules"`
}

func (h History) TableName() string {
	return "histories"
}

type HistoryGameRule struct {
	ID               uuid.UUID `db:"id" json:"-"`
	HistoryID        int       `db:"history_id" json:"-"`
	DefeatLostStones int       `db:"defeat_lost_stones" json:"defeat_lost_stones"`
	TurnTimeout      int       `db:"turn_timeout" json:"turn_timeout"`
	GameTimeout      int       `db:"game_timeout" json:"game_timeout"`
}

type Move struct {
	ID        int     `db:"id" json:"-"`
	HistoryID int     `db:"history_id" json:"-"`
	Player    int     `db:"player" json:"player"`
	Start     Cord    `db:"start_cord" json:"start"`
	End       Cord    `db:"end_cord" json:"end"`
	Dir       Cord    `db:"dir_cord" json:"dir"`
	GameTime  float64 `db:"game_time" json:"game_time"`
	TurnTime  float64 `db:"turn_time" json:"turn_time"`
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
	_, err := fmt.Sscanf(src, "%d=%d=%d", &c.X, &c.Y, &c.Z)
	return err
}
