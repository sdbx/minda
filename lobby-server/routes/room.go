package routes

import "lobby/servs/gameserv"

type room struct {
	Game *gameserv.GameServerServ `dim:"on"`
}
