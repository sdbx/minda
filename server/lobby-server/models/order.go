package models

import "time"

type OrderID struct {
	ID     int `db:"id"`
	LastID int `db:"last_id"`
}

type OrderLog struct {
	ID        int       `db:"id"`
	OrderID   int       `db:"order_id"`
	UserID    int       `db:"user_id"`
	Dif       int       `db:"dif"`
	CreatedAt time.Time `db:"created_at" json:"-"`
	UpdatedAt time.Time `db:"updated_at" json:"-"`
}

type SkinLog struct {
	ID        int       `db:"id"`
	UserID    int       `db:"user_id"`
	Dif       int       `db:"dif"`
	CreatedAt time.Time `db:"created_at" json:"-"`
	UpdatedAt time.Time `db:"updated_at" json:"-"`
}
