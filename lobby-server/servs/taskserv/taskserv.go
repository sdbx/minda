package taskserv

import (
	"encoding/json"
	"errors"
	"fmt"
	"lobby/models"
	"lobby/servs/discserv"
	"lobby/servs/redisserv"
	"lobby/utils"
	"sync"

	"github.com/garyburd/redigo/redis"
	"github.com/gobuffalo/uuid"
	"go.uber.org/zap"
)

const (
	redisGameQueueTmpl     = "task_game_queue_%s"
	redisLobbyQueue        = "task_lobby_queue"
	redisResultChannelTmpl = "task_result_chan_%s"
	resultTimeout          = 5
)

type TaskServ struct {
	Redis *redisserv.RedisServ   `dim:"on"`
	Disc  *discserv.DiscoverServ `dim:"on"`

	resMu sync.RWMutex
}

func Provide() *TaskServ {
	return &TaskServ{}
}

func (t *TaskServ) Init() error {
	go func() {
		err := utils.ListenQueue(t.Redis.Conn(),
			t.onQueueMessage,
			redisLobbyQueue,
		)
		if err != nil {
			utils.Log.Error("Error while listening queue", zap.Error(err))
		}
	}()
	return nil
}

func redisResultChannel(id string) string {
	return fmt.Sprintf(redisResultChannelTmpl, id)
}

func (t *TaskServ) onQueueMessage(buf []byte) {
	taskReq := models.TaskRequest{}
	err := json.Unmarshal(buf, &taskReq)
	if err != nil {
		utils.Log.Error("Error while parsing queue message", zap.Error(err))
		return
	}

	res := t.Excecute(taskReq.Task)

	buf, _ = json.Marshal(res)

	_, err = t.Redis.Conn().Do("RPUSH", redisResultChannel(taskReq.ID), buf)
	if err != nil {
		utils.Log.Error("Error while publishing the result", zap.Error(err))
		return
	}
}

func redisGameQueue(server string) string {
	return fmt.Sprintf(redisGameQueueTmpl, server)
}

func (t *TaskServ) Request(server string, task models.Task) (interface{}, error) {
	exists, err := t.Disc.ExistsGameServer(server)
	if err != nil {
		return nil, err
	}
	if !exists {
		return nil, errors.New("no such server")
	}

	id2, _ := uuid.NewV4()
	id := id2.String()

	taskReq := models.TaskRequest{
		ID:   id,
		Task: task,
	}
	buf, err := json.Marshal(taskReq)
	if err != nil {
		return nil, err
	}

	_, err = t.Redis.Conn().Do("RPUSH", redisGameQueue(server), buf)
	if err != nil {
		return nil, err
	}

	buf2, err := redis.ByteSlices(t.Redis.Conn().Do("BLPOP", redisResultChannel(id), resultTimeout))
	if err != nil {
		return nil, err
	}

	res := models.TaskResult{}
	err = json.Unmarshal(buf2[1], &res)
	if err != nil {
		return nil, err
	}

	if res.Error != nil {
		return nil, errors.New(*res.Error)
	}

	out := task.Out()
	err = json.Unmarshal([]byte(res.Value), &out)

	return out, err
}
