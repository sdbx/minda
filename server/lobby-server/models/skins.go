package models

type Skin struct {
	ID           int    `db:"id" json:"id"`
	Name         string `db:"name" json:"name"`
	BlackPicture string `db:"black_picture" json:"black_picture"`
	WhitePicture string `db:"white_picture" json:"white_picture"`
}
