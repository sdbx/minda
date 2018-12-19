package authserv

import (
	"errors"
	"lobby/models"
)

type AuthServ struct {
}

func Provide() *AuthServ {
	return &AuthServ{}
}

func (a *AuthServ) GetUser(token string) (models.User, error) {
	if token == "black" {
		return models.User{
			ID:       1,
			Username: "흑우",
		}, nil
	}
	if token == "white" {
		return models.User{
			ID:       2,
			Username: "백우",
		}, nil
	}
	return models.User{}, errors.New("no such user")
}
