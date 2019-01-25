package taskserv

import (
	"fmt"
	"lobby/models"
	"lobby/utils"
)

func (t *TaskServ) Execute(task models.Task) models.TaskResult {
	switch task := task.(type) {
	case *models.CompleteGameTask:
		fmt.Println(task)
		return models.TaskResult{
			Value: "{}",
		}
	default:
		return models.TaskResult{
			Error: utils.NewString("Unsupported task"),
		}
	}
}
