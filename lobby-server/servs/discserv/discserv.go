package discserv

import (
	"encoding/json"
	"lobby/models"
	"lobby/servs/redisserv"
	"lobby/utils"
	"sort"
	"time"

	"github.com/garyburd/redigo/redis"
)

const (
	redisServerHash = "game_server_hash"
	updateTick      = time.Second / 10
	garbageTime     = time.Second * 10
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

func (r Rooms) Get(id string) *models.Room {
	for key := range r {
		if r[key].ID == id {
			return &r[key]
		}
	}
	return nil
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
			for _, server := range servers {
				if server.LastPing.Add(garbageTime).Before(time.Now().UTC()) {
					g.Redis.Conn().Do("HDEL", redisServerHash, server.Name)
				}
			}
			rooms, err := g.FetchRooms(false, false)
			if err == nil {
				g.rooms = rooms
			}
		}
	}
}

func (g *DiscoverServ) FetchRooms(empty bool, private bool) (Rooms, error) {
	servers, err := g.ListGameServers()
	if err != nil {
		return nil, err
	}
	rooms := Rooms{}
	for _, server := range servers {
		for _, room := range server.Rooms {
			if (len(room.Users) != 0 || empty) && (room.Conf.Open || private) {
				rooms = append(rooms, room)
			}
		}
	}
	sort.Sort(rooms)
	return rooms, nil
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
		for i := range s.Rooms {
			s.Rooms[i].Server = s.Name
		}
		out = append(out, s)
	}
	return out, nil
}

func (g *DiscoverServ) ExistsGameServer(name string) (bool, error) {
	conn := g.Redis.Conn()
	return redis.Bool(conn.Do("HEXISTS", redisServerHash, name))
}

func (g *DiscoverServ) CreateRoomID() (string, error) {
	rooms, err := g.FetchRooms(true, true)
	if err != nil {
		return "", err
	}
L:
	for {
		out := utils.RandString(6)
		for _, room := range rooms {
			if room.ID == out {
				continue L
			}
		}
		return out, nil
	}
}
