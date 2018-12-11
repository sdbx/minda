package discserv

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

type DiscoverServ struct {
	Redis *redisserv.RedisServ `dim:"on"`

	rooms Rooms
}

func Provide() *DiscoverServ {
	return &DiscoverServ{}
}

func (g *DiscoverServ) Init() error {
	go g.update()
	return nil
}

func (g *DiscoverServ) update() {
	ticker := time.NewTicker(updateTick)
	for {
		select {
		case <-ticker.C:
			servers, err := g.ListGameServers()
			if err != nil {
			}
			rooms := Rooms{}
			for _, server := range servers {
				if server.LastPing.Add(garbageTime).Before(time.Now().UTC()) {
					g.Redis.Conn().Do("HDEL", redisServerHash, server.Name)
				}
				rooms = append(rooms, server.Rooms...)
			}
			sort.Sort(rooms)
			g.rooms = rooms
		}
	}
}

func (g *DiscoverServ) GetRooms() Rooms {
	return g.rooms
}

func (g *DiscoverServ) ListGameServers() ([]models.GameServer, error) {
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

func (g *DiscoverServ) ExistsGameServer(name string) (bool, error) {
	conn := g.Redis.Conn()
	return redis.Bool(conn.Do("HEXISTS", redisServerHash, name))
}
