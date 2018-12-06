package taskserv

type TaskServ struct {
}

func Provide() *TaskServ {
	return &TaskServ{}
}
