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
	JoinRoomKind   = "join-room"
	KickUserKind   = "kick-user"
	DeleteRoomKind = "delete-room"
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
	case JoinRoomKind:
		r.Task = &JoinRoomTask{}
	case KickUserKind:
		r.Task = &KickUserTask{}
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
	Conf   RoomConf `json:"conf"`
	UserID int      `json:"user_id"`
}

func (CreateRoomTask) Out() interface{} {
	return &LobbyRoomResult{}
}

func (CreateRoomTask) Kind() string {
	return CreateRoomKind
}

func (CreateRoomTask) sealedTask() {}

type JoinRoomTask struct {
	RoomID string `json:"room_id"`
	UserID int    `json:"user_id"`
}

func (JoinRoomTask) Out() interface{} {
	return &LobbyRoomResult{}
}

func (JoinRoomTask) Kind() string {
	return JoinRoomKind
}

func (JoinRoomTask) sealedTask() {}

type KickUserTask struct {
	RoomID string `json:"room_id"`
	UserID int    `json:"user_id"`
}

func (KickUserTask) Out() interface{} {
	return &UnitResult{}
}

func (KickUserTask) Kind() string {
	return KickUserKind
}

func (KickUserTask) sealedTask() {}

type DeleteRoomTask struct {
	RoomID string `json:"room_id"`
}

func (DeleteRoomTask) Out() interface{} {
	return &UnitResult{}
}

func (DeleteRoomTask) Kind() string {
	return DeleteRoomKind
}

func (DeleteRoomTask) sealedTask() {}

type LobbyRoomResult struct {
	Invite string `json:"invite"`
	Addr   string `json:"addr"`
}

type UnitResult struct {
}
