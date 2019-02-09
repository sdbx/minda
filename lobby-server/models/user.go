package models

import (
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
	Permission UserPermission `has_one:"user_permission" json:"permission"`
	Inventory  UserInventory  `has_one:"user_inventory" json:"inventory"`
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
