package taskserv

import (
	"lobby/models"
	"lobby/utils"
)

func (t *TaskServ) Execute(task models.Task) models.TaskResult {
	switch task := task.(type) {
	case *models.CompleteGameTask:
		loser := -1
		if task.Loser == "black" {
			loser = task.Black
		} else {
			loser = task.White
		}
		history := models.History{
			Black: task.Black,
			White: task.White,
			Map:   task.Map,
			Moves: task.Moves,
			Loser: loser,
			Cause: task.Cause,
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
