package models

import "time"

type User struct {
	ID        int       `db:"id"`
	CreatedAt time.Time `db:"created_at"`
	UpdatedAt time.Time `db:"updated_at"`
	Username  string    `db:"username"`
}

type OauthUsers struct {
	OauthID   string    `db:"oauth_id"`
	Provider  string    `db:"prodiver"`
	UserID    int       `db:"user_id"`
	CreatedAt time.Time `db:"created_at"`
	UpdatedAt time.Time `db:"updated_at"`
}
