package models

type Result struct {
	ID    string  `json:"id"`
	Error *string `json:"error"`
	Value []byte  `json:"value"`
}

type Task struct {
	ID    string `json:"id"`
	Type  string `json:"type"`
	Param []byte `json:"param"`
}
