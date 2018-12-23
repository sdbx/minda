package dbserv

import "github.com/gobuffalo/pop"

type DBServ struct {
	*pop.Connection
}

func Provide() (*DBServ, error) {
	c, err := pop.Connect("minda")
	if err != nil {
		return nil, err
	}
	return &DBServ{
		Connection: c,
	}, nil 
}