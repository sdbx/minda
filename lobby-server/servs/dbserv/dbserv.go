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

func (db *DBServ) Init() error {
	mig, err := pop.NewFileMigrator("migrations", db.Connection)
	if err != nil {
		return err
	}
	return mig.Up()
}