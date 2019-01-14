package models

import (
	"reflect"
	"strconv"
	"strings"

	"github.com/gobuffalo/uuid"
)

type Map struct {
	ID      int       `json:"id"`
	Name    string    `json:"name"`
	Payload MapString `json:"payload"`
	Public  bool      `json:"-"`
}

type UserMap struct {
	ID     uuid.UUID `db:"id" json:"-"`
	UserID int       `db:"user_id"`
	MapID  int       `db:"map_id"`
}

type MapString string

func validateMapString(field reflect.Value) interface{} {
	if str, ok := field.Interface().(MapString); ok {
		tmp := strings.Split(string(str), "#")
		var arr [][]int
		for i := range tmp {
			tmp2 := strings.Split(tmp[i], "@")
			if i != 0 && len(arr[0]) != len(tmp2) {
				return nil
			}

			var tmp3 []int
			for j := range tmp2 {
				n, err := strconv.Atoi(tmp2[j])
				if err != nil {
					return nil
				}
				tmp3 = append(tmp3, n)
			}
			arr = append(arr, tmp3)
		}
		return arr
	}
	return nil
}
