package models

import (
	"errors"
	"strconv"
	"strings"

	"github.com/gobuffalo/uuid"
)

const (
	StoneBlank = iota
	StoneBlack
	StoneWhite
)

type Map struct {
	ID      int       `db:"id" json:"id"`
	Name    string    `db:"name" json:"name"`
	Payload MapString `db:"payload" json:"payload"`
	Public  bool      `db:"public" json:"-"`
}

type UserMap struct {
	ID     uuid.UUID `db:"id"`
	UserID int       `db:"user_id"`
	MapID  int       `db:"map_id"`
}

type MapString string

func (str MapString) Parse() ([][]int, error) {
	tmp := strings.Split(string(str), "#")
	var arr [][]int
	for i := range tmp {
		tmp2 := strings.Split(tmp[i], "@")
		if i != 0 && len(arr[0]) != len(tmp2) {
			return nil, errors.New("invalid map string")
		}

		var tmp3 []int
		for j := range tmp2 {
			n, err := strconv.Atoi(tmp2[j])
			if err != nil {
				return nil, errors.New("invalid map string")
			}
			tmp3 = append(tmp3, n)
		}
		arr = append(arr, tmp3)
	}
	if len(arr) != 9 {
		return nil, errors.New("invalid map string")
	}
	return arr, nil
}

// black, white
func countStones(board [][]int) (black int, white int) {
	for _, i := range board {
		for _, j := range i {
			if j == StoneBlack {
				black++
			} else if j == StoneWhite {
				white++
			}
		}
	}
	return
}
