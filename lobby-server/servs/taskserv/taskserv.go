package taskserv

import (
	"encoding/json"
	"lobby/models"
	"lobby/servs/redisserv"
	"lobby/utils"
	"sync"
)

const redisLobbyQueue = "task_lobby_queue"
const redisResultPubSub = "task_result_pub_sub"

type TaskServ struct {
	Redis *redisserv.RedisServ `dim:"on"`

	resMu sync.RWMutex
	res   map[string]chan models.Result
}

func Provide() *TaskServ {
	return &TaskServ{}
}

func (t *TaskServ) Init() {
	go func() {
		err := utils.ListenPubSub(t.Redis.Conn(),
			t.onStart,
			t.onResultMessage,
			redisResultPubSub,
		)
		if err != nil {
		}
	}()

	go func() {
		err := utils.ListenQueue(t.Redis.Conn(),
			t.onQueueMessage,
			redisLobbyQueue,
		)
		if err != nil {
		}
	}()
}

func (t *TaskServ) notifyResult(res models.Result) error {
	buf, err := json.Marshal(res)
	if err != nil {
		return err
	}
	return t.Redis.Conn().Send("PUBLISH", redisResultPubSub, buf)
}

func (t *TaskServ) onResultMessage(channel string, buf []byte) {
	res := models.Result{}
	err := json.Unmarshal(buf, &res)
	if err != nil {

	}
	t.resMu.RLock()
	defer t.resMu.RUnlock()
	if c, ok := t.res[res.ID]; ok {
		c <- res
		delete(t.res, res.ID)
	}
}

func (t *TaskServ) onQueueMessage(buf []byte) {
	task := models.Task{}
	err := json.Unmarshal(buf, &task)
	if err != nil {
	}
	t.Excecute(task)
}

func (t *TaskServ) onStart() {

}

func (t *TaskServ) Excecute(task models.Task) models.Result {
	return models.Result{}
}

func (t *TaskServ) Request(server string, task models.Task) (chan models.Result, error) {
	return nil, nil
}
