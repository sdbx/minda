package matchserv

import (
	"encoding/json"
	"errors"
	"fmt"

	"lobby/models"
	"lobby/servs/discserv"
	"lobby/servs/redisserv"
	"lobby/servs/taskserv"
	"log"
	"math/rand"
	"strconv"
	"time"

	"github.com/garyburd/redigo/redis"
)

var (
	ErrNotFound = errors.New("not found")
)

const (
	redisMatchRequests = "match_requests"
	redisMatchTmpl     = "match_%d"
)

func redisMatch(userid int) string {
	return fmt.Sprintf(redisMatchTmpl, userid)
}

type MatchServ struct {
	Task     *taskserv.TaskServ     `dim:"on"`
	Disc     *discserv.DiscoverServ `dim:"on"`
	Redis    *redisserv.RedisServ   `dim:"on"`
	enable   bool
	interval time.Duration
}

func (MatchServ) ConfigName() string {
	return "match"
}

type MatchServConfig struct {
	Enable          bool `yaml:"enable"`
	IntervalSeconds int  `yaml:"interval_seconds"`
}

type matchRequest struct {
	CreatedAt time.Time
	UserID    int
	R         int
}

type match struct {
	RoomID string
}

func Provide(conf MatchServConfig) *MatchServ {
	return &MatchServ{
		enable:   conf.Enable,
		interval: time.Duration(conf.IntervalSeconds) * time.Second,
	}
}

func (m *MatchServ) Init() error {
	if m.enable {
		go func() {
			for range time.Tick(m.interval) {
				m.update()
			}
		}()
	}
	return nil
}

func (m *MatchServ) FindMatch(userid int) (string, error) {
	mm, err := m.getMatch(userid)
	if err == nil {
		return mm.RoomID, nil
	}

	err = m.setMatchRequest(userid, matchRequest{
		CreatedAt: time.Now().UTC(),
		UserID:    userid,
		R:         0,
	})

	if err != nil {
		return "", err
	}
	return "", ErrNotFound
}

func (m *MatchServ) CancelSearching(userid int) {
	m.removeMatchRequest(userid)
}

func (m *MatchServ) setMatchRequest(userid int, req matchRequest) error {
	buf, err := json.Marshal(req)
	if err != nil {
		return err
	}

	_, err = m.Redis.Conn().Do("HSET", redisMatchRequests, strconv.Itoa(userid), string(buf))
	if err != nil {
		return err
	}
	return nil
}

func (m *MatchServ) getMatch(userid int) (match, error) {
	buf, err := redis.String(m.Redis.Conn().Do("GET", redisMatch(userid)))
	if err != nil {
		return match{}, err
	}
	var out match
	err = json.Unmarshal([]byte(buf), &out)
	if err != nil {
		return match{}, err
	}
	return out, nil
}

func (m *MatchServ) setMatch(userid int, mm match) error {
	buf, err := json.Marshal(mm)
	if err != nil {
		return err
	}
	_, err = m.Redis.Conn().Do("SET", redisMatch(userid), string(buf))
	if err != nil {
		return err
	}
	_, err = m.Redis.Conn().Do("EXPIRE", redisMatch(userid), 10)
	return nil
}

func (m *MatchServ) removeMatchRequest(userid int) error {
	_, err := m.Redis.Conn().Do("HDEL", redisMatchRequests, strconv.Itoa(userid))
	if err != nil {
		return err
	}
	return nil
}

func (m *MatchServ) getMatchRequests() ([]matchRequest, error) {
	raw, err := redis.StringMap(m.Redis.Conn().Do("HGETALL", redisMatchRequests))
	if err != nil {
		return nil, err
	}
	out := make([]matchRequest, 0, len(raw))
	for key, val := range raw {
		var obj matchRequest
		err = json.Unmarshal([]byte(val), &obj)
		if err != nil {
			log.Println(err)
			continue
		}
		if time.Now().UTC().Sub(obj.CreatedAt) >= 10*time.Second {
			_, err = m.Redis.Conn().Do("HDEL", redisMatchRequests, key)
			continue
		}
		out = append(out, obj)
	}
	return out, nil
}

func (m *MatchServ) createRoom(black int, white int) (string, error) {
	servers, err := m.Disc.ListGameServers()
	if err != nil {
		return "", err
	}
	id, err := m.Disc.CreateRoomID()
	if err != nil {
		return "", err
	}
	conf := models.RoomConf{
		Name:  "Rank room",
		Open:  false,
		White: -1,
		Black: -1,
		King:  -1,
		Map:   "0@0@0@0@0@0@0@1@1#0@0@0@0@0@0@0@1@1#0@0@0@0@0@0@1@1@1#0@2@0@0@0@0@1@1@1#2@2@2@0@0@0@1@1@1#2@2@2@0@0@0@0@1@0#2@2@2@0@0@0@0@0@0#2@2@0@0@0@0@0@0@0#2@2@0@0@0@0@0@0@0",
		GameRule: models.GameRule{
			GameTimeout:      600,
			TurnTimeout:      60,
			DefeatLostStones: 6,
		},
	}
	_, err = m.Task.Request(servers[rand.Intn(len(servers))].Name, &models.CreateRoomTask{
		RoomID: id,
		Conf:   conf,
		UserID: -1,
		Rank: &models.RoomRank{
			Time:  10 * 1000,
			Black: black,
			White: white,
		},
	})
	if err != nil {
		return "", err
	}
	return id, nil
}

func (m *MatchServ) update() {
	reqs, err := m.getMatchRequests()
	if err != nil {
		log.Println(err)
		return
	}

	reqs2 := make([]matchRequest, 0, len(reqs))
	for _, req := range reqs {
		_, err := m.getMatch(req.UserID)
		if err != nil {
			reqs2 = append(reqs2, req)
		}
	}
	reqs = reqs2
	for i := 0; i < len(reqs); i += 2 {
		black := reqs[i].UserID
		white := reqs[i+1].UserID
		id, err := m.createRoom(black, white)
		if err != nil {
			log.Println(err)
			continue
		}
		mm := match{
			RoomID: id,
		}
		err = m.setMatch(black, mm)
		if err != nil {
			log.Println(err)
			continue
		}
		err = m.setMatch(white, mm)
		if err != nil {
			log.Println(err)
			continue
		}
	}
}
