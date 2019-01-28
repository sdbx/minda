package taskserv

import (
	"lobby/models"
	"lobby/utils"
)

func (t *TaskServ) Execute(task models.Task) models.TaskResult {
	switch task := task.(type) {
	case *models.CompleteGameTask:
		history := models.History{
			Black: task.Black,
			White: task.White,
			Map:   task.Map,
			Moves: task.Moves,
		}
		err := t.DB.Eager().Create(&history)
		if err != nil {
			return models.TaskResult{
				Error: utils.NewString(err.Error()),
			}
		}
		return models.TaskResult{
			Value: "{}",
		}
	default:
		return models.TaskResult{
			Error: utils.NewString("Unsupported task"),
		}
	}
}
