package models

import "time"

type GameServer struct {
	Name     string    `json:"name"`
	Addr     string    `json:"addr"`
	Rooms    []Room    `json:"rooms"`
	LastPing time.Time `json:"last_ping"`
}

type Room struct {
	ID        string    `json:"id"`
	CreatedAt time.Time `json:"created_at"`
	Conf      RoomConf  `json:"conf"`
	Users     []int     `json:"users"`
	Ingame    bool      `json:"ingame"`
	Server    string    `json:"-"`
}

type RoomConf struct {
	Name  string `json:"name"`
	King  int    `json:"king"`
	Black int    `json:"black"`
	White int    `json:"white"`
}

type Picture struct {
	ID      int    `db:"id"`
	Payload []byte `db:"payload"`
}
