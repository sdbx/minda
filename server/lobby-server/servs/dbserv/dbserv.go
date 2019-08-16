package dbserv

import (
	"lobby/models"

	"github.com/gobuffalo/packr/v2"
	"github.com/gobuffalo/pop"
)

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
	box := packr.New("migrations", "../../migrations")
	mig, err := pop.NewMigrationBox(box, db.Connection)
	if err != nil {
		return err
	}

	return mig.Up()
}

func (db *DBServ) GetSkinsOfUser(user int) ([]models.Skin, error) {
	var out []models.Skin
	err := db.Q().
		InnerJoin("user_skins", "skins.id = user_skins.skin_id").
		Where("user_skins.user_id = ?", user).All(&out)
	return out, err
}
