package models

import (
	"lobby/gicko"
	"time"

	"github.com/gobuffalo/uuid"

	"github.com/gobuffalo/pop/nulls"
)

type User struct {
	ID         int            `db:"id" json:"id"`
	Username   string         `db:"username" json:"username"`
	Picture    nulls.String   `db:"picture" json:"picture"`
	CreatedAt  time.Time      `db:"created_at" json:"-"`
	UpdatedAt  time.Time      `db:"updated_at" json:"-"`
	Rating     UserRating     `has_one:"user_rating" json:"rating"`
	Permission UserPermission `has_one:"user_permission" json:"permission"`
	Inventory  UserInventory  `has_one:"user_inventory" json:"inventory"`
}

type UserRating struct {
	ID     uuid.UUID `db:"id" json:"-"`
	UserID int       `db:"user_id" json:"-"`
	R      float64   `db:"r" json:"score"`
	RD     float64   `db:"rd" json:"-"`
	V      float64   `db:"v" json:"-"`
}

func (u *UserRating) ToPlayer() gicko.Player {
	return gicko.Player{
		R:  u.R,
		RD: u.RD,
		V:  u.V,
	}
}

func (u *UserRating) Update(p gicko.Player) {
	u.R = p.R
	u.RD = p.RD
	u.V = p.V
}

type UserPermission struct {
	ID     uuid.UUID `db:"id" json:"-"`
	UserID int       `db:"user_id" json:"-"`
	Admin  bool      `db:"admin" json:"admin"`
}

type UserInventory struct {
	ID           uuid.UUID `db:"id" json:"-"`
	UserID       int       `db:"user_id" json:"-"`
	OneColorSkin int       `db:"one_color_skin" json:"one_color_skin"`
	TwoColorSkin int       `db:"two_color_skin" json:"two_color_skin"`
	CurrentSkin  nulls.Int `db:"current_skin" json:"current_skin"`
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

type UserSkin struct {
	ID     uuid.UUID `db:"id"`
	UserID int       `db:"user_id"`
	SkinID int       `db:"skin_id"`
}
