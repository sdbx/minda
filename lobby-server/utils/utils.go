package utils

import (
	"errors"
	"math/rand"
	"time"

	"go.uber.org/zap"

	"github.com/garyburd/redigo/redis"
)

var (
	ErrNotExists    = errors.New("no such resource")
	ErrNoGameServer = errors.New("no gameserver available")
)

var Log, _ = zap.NewProduction()

func init() {
	rand.Seed(time.Now().UnixNano())
}

func ListenPubSub(conn redis.Conn,
	onStart func(),
	onMessage func(channel string, data []byte),
	channels ...string) error {
	psc := redis.PubSubConn{Conn: conn}

	if err := psc.Subscribe(redis.Args{}.AddFlat(channels)...); err != nil {
		return err
	}

	done := make(chan error, 1)

	go func() {
		for {
			switch n := psc.Receive().(type) {
			case error:
				done <- n
				return
			case redis.Message:
				onMessage(n.Channel, n.Data)
			case redis.Subscription:
				switch n.Count {
				case len(channels):
					onStart()
				case 0:
					done <- nil
					return
				}
			}
		}
	}()

	return <-done
}

func ListenQueue(conn redis.Conn, onMessage func(buf []byte), name string) error {
	done := make(chan error, 1)
	go func() {
		for {
			buf, err := redis.Bytes(conn.Do("BLPOP", name, 0))
			if err != nil {
				done <- err
				return
			}
			onMessage(buf)
		}
	}()
	return <-done
}

func NewString(str string) *string {
	return &str
}

const letterBytes = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"

func RandString(n int) string {
	b := make([]byte, n)
	for i := range b {
		b[i] = letterBytes[rand.Intn(len(letterBytes))]
	}
	return string(b)
}
