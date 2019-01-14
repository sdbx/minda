package models

type Map struct {
	ID      int    `json:"id"`
	Name    string `json:"name"`
	User    int    `json:"user"`
	Payload string `json:"payload"`
	Public  bool   `json:"-"`
}
