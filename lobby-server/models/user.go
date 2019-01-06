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
