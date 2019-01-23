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
	Name     string    `json:"name"`
	King     int       `json:"king"`
	Black    int       `json:"black"`
	White    int       `json:"white"`
	Map      MapString `json:"map"`
	GameRule GameRule  `json:"game_rule"`
}

type GameRule struct {
	DefeatLostStones int `json:"defeat_lost_stones"`
	TurnTimeout      int `json:"turn_timeout"`
	GameTimeout      int `json:"game_timeout"`
}

func (r *RoomConf) Validate() bool {
	board, err := r.Map.Parse()
	if err != nil {
		return false
	}
	black, white := countStones(board)
	if black != white ||
		r.GameRule.TurnTimeout == 0 ||
		r.GameRule.GameTimeout == 0 ||
		r.GameRule.DefeatLostStones == 0 ||
		r.GameRule.DefeatLostStones > black ||
		r.King == -1 ||
		r.Black != -1 ||
		r.White != -1 {
		return false
	}
	return true
}

type Picture struct {
	ID      int    `db:"id"`
	Payload []byte `db:"payload"`
}
