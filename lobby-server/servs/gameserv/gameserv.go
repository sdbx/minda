package gameserv

import (
	"encoding/json"
	"lobby/models"
	"lobby/servs/redisserv"
	"sort"
	"time"

	"github.com/garyburd/redigo/redis"
)

const (
	redisServerHash = "game_server_hash"
	updateTick      = time.Second
	garbageTime     = time.Second * 20
)

type Rooms []models.Room

func (r Rooms) Swap(i, j int) {
	r[i], r[j] = r[j], r[i]
}

func (r Rooms) Less(i, j int) bool {
	return r[i].CreatedAt.Before(r[j].CreatedAt)
}

func (r Rooms) Len() int {
	return len(r)
}

type GameServerServ struct {
	Redis *redisserv.RedisServ `dim:"on"`

	rooms Rooms
}

func Provide() *GameServerServ {
	return &GameServerServ{}
}

func (g *GameServerServ) Init() {
	go g.update()
}

func (g *GameServerServ) update() {
	ticker := time.NewTicker(updateTick)
	for {
		select {
		case <-ticker.C:
			servers, err := g.ListGameServers()
			if err != nil {
			}
			rooms := Rooms{}
			for _, server := range servers {
				if server.LastPing.Add(garbageTime).Before(time.Now()) {
					g.Redis.Conn().Send("HDEL", redisServerHash, server.Name)
				}
				rooms = append(rooms, server.Rooms...)
			}
			sort.Sort(rooms)
			g.rooms = rooms
		}
	}
}

func (g *GameServerServ) GetRooms() Rooms {
	return g.rooms
}

func (g *GameServerServ) ListGameServers() ([]models.GameServer, error) {
	conn := g.Redis.Conn()
	vals, err := redis.ByteSlices(conn.Do("HVALS", redisServerHash))
	if err != nil {
		return nil, err
	}

	out := make([]models.GameServer, 0, len(vals))
	for _, val := range vals {
		s := models.GameServer{}
		err = json.Unmarshal(val, &s)
		if err != nil {
			return nil, err
		}
		out = append(out, s)
	}
	return out, nil
}

func (g *GameServerServ) ExistsGameServer(name string) (bool, error) {
	conn := g.Redis.Conn()
	return redis.Bool(conn.Do("HEXISTS", redisServerHash, name))
}
