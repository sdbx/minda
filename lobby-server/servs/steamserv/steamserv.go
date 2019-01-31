package steamserv

import (
	"lobby/steam"
)

type SteamServ struct {
	*steam.Steam
}

type SteamServConf struct {
	AppID string `yaml:"app_id"`
	Key   string `yaml:"key"`
}

func Provide(conf SteamServConf) (*SteamServ, error) {
	return &SteamServ{
		Steam: steam.New(conf.AppID, conf.Key),
	}, nil
}

func (s SteamServ) ConfigName() string {
	return "steam"
}
