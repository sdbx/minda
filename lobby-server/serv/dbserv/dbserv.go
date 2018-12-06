package dbserv

import "github.com/gobuffalo/pop"

type DBServ struct {
	c *pop.Connection
}

type DBServConf struct {
	Stage string `yaml:"stage"`
}

func Provide(conf DBServConf) (*DBServ, error) {
	c, err := pop.Connect(conf.Stage)
	if err != nil {
		return nil, err
	}

	return &DBServ {
		c: c,
	}, nil
}