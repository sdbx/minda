package payserv

import (
	"lobby/servs/dbserv"
	"lobby/servs/steamserv"
)

type PayServ struct {
	Steam *steamserv.SteamServ `dim:"on"`
	DB    *dbserv.DBServ       `dim:"on"`
}

func (p *PayServ) InitSteamTxn(userid int) (int, error) {
	var item models.OAuthUser
	err := p.DB.Q().Where("user_id = ?", userid).First(&item)
}
