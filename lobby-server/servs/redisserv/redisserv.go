package redisserv

import (
	"time"

	"github.com/garyburd/redigo/redis"
)

const IdleTimeout = 240 * time.Second

type RedisServ struct {
	pool *redis.Pool
}

type RedisServConf struct {
	Addr    string `yaml:"addr"`
	MaxIdle int    `yaml:"max_idle"`
}

func Provide(conf RedisServConf) (*RedisServ, error) {
	_, err := redis.Dial("tcp", conf.Addr)
	if err != nil {
		return nil, err
	}
	pool := &redis.Pool{
		MaxIdle:     conf.MaxIdle,
		IdleTimeout: IdleTimeout,
		Dial:        func() (redis.Conn, error) { return redis.Dial("tcp", conf.Addr) },
	}
	return &RedisServ{
		pool: pool,
	}, nil
}

func (RedisServ) ConfigName() string {
	return "redis"
}

func (r *RedisServ) GetPool() *redis.Pool {
	return r.pool
}

func (r *RedisServ) Conn() redis.Conn {
	return r.pool.Get()
}
