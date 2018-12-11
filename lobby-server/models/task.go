package models

type Result struct {
	Error *string `json:"error"`
	Value string  `json:"value"`
}

type TaskRequest struct {
	ID   string `json:"id"`
	Task Task   `json:"task"`
}

type Task interface{}

func UnmarshalTaskRequest(buf []byte) (TaskRequest, error) {
	return nil, nil
}

func MarshalTaskRequest(t TaskRequest) ([]byte, error) {

}
