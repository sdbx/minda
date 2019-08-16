package taskserv

import (
	"encoding/json"
	"lobby/gicko"
	"lobby/models"
	"lobby/utils"
)

func (t *TaskServ) Execute(task models.Task) models.TaskResult {
	switch task := task.(type) {
	case *models.CompleteGameTask:
		loser := -1
		winner := -1
		if task.Loser == "black" {
			loser = task.Black
			winner = task.White
		} else {
			loser = task.White
			winner = task.Black
		}
		history := models.History{
			Black: task.Black,
			White: task.White,
			Map:   task.Map,
			Moves: task.Moves,
			Rule: models.HistoryGameRule{
				DefeatLostStones: task.GameRule.DefeatLostStones,
				TurnTimeout:      task.GameRule.TurnTimeout,
				GameTimeout:      task.GameRule.GameTimeout,
			},
			Loser: loser,
			Cause: task.Cause,
		}
		err := t.DB.Eager().Create(&history)
		if err != nil {
			return models.TaskResult{
				Error: utils.NewString(err.Error()),
			}
		}
		u, err := t.Auth.GetUser(winner)
		if err != nil {
			return models.TaskResult{
				Error: utils.NewString(err.Error()),
			}
		}

		u2, err := t.Auth.GetUser(loser)
		if err != nil {
			return models.TaskResult{
				Error: utils.NewString(err.Error()),
			}
		}
		r, r2 := u.Rating, u2.Rating
		p, p2 := r.ToPlayer(), r2.ToPlayer()
		pp, delta1 := gicko.Update(p, p2, 1, 0.5)
		r.Update(pp)
		err = t.DB.Update(&r)
		if err != nil {
			return models.TaskResult{
				Error: utils.NewString(err.Error()),
			}
		}

		p2p, delta2 := gicko.Update(p2, p, 0, 0.5)
		r2.Update(p2p)
		err = t.DB.Update(&r2)
		if err != nil {
			return models.TaskResult{
				Error: utils.NewString(err.Error()),
			}
		}
		out := struct {
			WinnerDelta float64 `json:"winner_delta"`
			LoserDelta  float64 `json:"loser_delta"`
		}{
			WinnerDelta: delta1,
			LoserDelta:  delta2,
		}

		buf, err := json.Marshal(out)
		if err != nil {
			return models.TaskResult{
				Error: utils.NewString(err.Error()),
			}
		}

		return models.TaskResult{
			Value: string(buf),
		}
	default:
		return models.TaskResult{
			Error: utils.NewString("Unsupported task"),
		}
	}
}
