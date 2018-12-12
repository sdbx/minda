package models

import (
	"encoding/json"
	"errors"
)

type Result struct {
	Error *string `json:"error"`
	Value string  `json:"value"`
}

type TaskRequest struct {
	ID   string `json:"id"`
	Task Task   `json:"task"`
}

const (
	CreateRoomKind = "create-room"
)

type Task interface {
	Kind() string
	Out() interface{}
	sealedTask()
}

func (r *TaskRequest) UnmarshalJSON(b []byte) error {
	obj := struct {
		ID   string `json:"id"`
		Task []byte `json:"task"`
	}{}
	err := json.Unmarshal(b, &obj)
	if err != nil {
		return err
	}

	r.ID = obj.ID

	obj2 := struct {
		Kind string `json:"kind"`
	}{}
	err = json.Unmarshal(obj.Task, &obj2)
	if err != nil {
		return err
	}

	switch obj2.Kind {
	case CreateRoomKind:
		r.Task = &CreateRoomTask{}
	default:
		return errors.New("unknown task kind")
	}

	return json.Unmarshal(obj.Task, r.Task)
}

func (r TaskRequest) MarshalJSON() ([]byte, error) {
	obj := struct {
		ID   string                 `json:"id"`
		Task map[string]interface{} `json:"task"`
	}{}
	obj.ID = r.ID
	buf, err := json.Marshal(r.Task)
	if err != nil {
		return nil, err
	}

	err = json.Unmarshal(buf, &obj.Task)
	if err != nil {
		return nil, err
	}

	obj.Task["kind"] = r.Task.Kind()
	return json.Marshal(obj)
}

type CreateRoomTask struct {
	Name string `json:"name"`
	User User   `json:"user"`
}

func (CreateRoomTask) Out() interface{} {
	return &CreateRoomResult{}
}

func (CreateRoomTask) Kind() string {
	return CreateRoomKind
}

func (CreateRoomTask) sealedTask() {}

type CreateRoomResult struct {
	ID   string `json:"id"`
	Addr string `json:"addr"`
}
