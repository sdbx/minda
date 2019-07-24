package payserv

import (
	"fmt"
	"lobby/models"
	"lobby/servs/authserv"
	"lobby/servs/dbserv"
	"lobby/servs/redisserv"
	"lobby/servs/steamserv"
	"lobby/steam"
	"strings"

	"github.com/garyburd/redigo/redis"
)

const (
	redisOrderTmpl   = "order_%d"
	redisOrderIDPool = "order_id_pool"
	redisUniqueID    = "order_id"
)

func redisOrder(orderid int) string {
	return fmt.Sprintf(redisOrderTmpl, orderid)
}

type PayServ struct {
	Auth  *authserv.AuthServ   `dim:"on"`
	Redis *redisserv.RedisServ `dim:"on"`
	Steam *steamserv.SteamServ `dim:"on"`
	DB    *dbserv.DBServ       `dim:"on"`
}

func Provide() *PayServ {
	return &PayServ{}
}

func (p *PayServ) InitOrder(userid int) error {
	user, err := p.Auth.GetOAuthUserByUser(userid)
	if err != nil {
		return err
	}

	id, err := p.getID()
	if err != nil {
		return err
	}

	_, err = p.Steam.InitTxn(id, user.ID, []steam.Item{
		steam.Item{
			ID:     1,
			Name:   "Minda skin",
			Qty:    1,
			Amount: 100,
		},
	})
	if err != nil {
		if !strings.Contains(err.Error(), "has already been used by") {
			p.returnID(id)
		}
		return err
	}

	err = p.createOrder(id, userid)
	if err != nil {
		return err
	}

	return nil
}

func (p *PayServ) getID() (int, error) {
	out, err := redis.Int(p.Redis.Conn().Do("RPOP", redisOrderIDPool))
	if err == nil {
		return out, nil
	}

	var id models.OrderID
	err = p.DB.Q().Where("id = 1").First(&id)
	if err != nil {
		return 0, err
	}

	id.LastID++
	err = p.DB.Update(&id)
	if err != nil {
		return 0, err
	}

	return id.LastID, nil
}

func (p *PayServ) returnID(orderid int) error {
	_, err := p.Redis.Conn().Do("LPUSH", redisOrderIDPool, orderid)
	if err != nil {
		return err
	}

	return nil
}

func (p *PayServ) FinalizeOrder(orderid int) error {
	userid, err := redis.Int(p.Redis.Conn().Do("GET", redisOrder(orderid)))
	if err != nil {
		return err
	}

	user, err := p.Auth.GetUser(userid)
	if err != nil {
		return err
	}

	err = p.Steam.FinalizeTxn(orderid)
	if err != nil {
		return err
	}

	err = p.DB.Create(&models.OrderLog{
		UserID:  user.ID,
		OrderID: orderid,
		Dif:     1,
	})
	if err != nil {
		return err
	}

	err = p.DB.Create(&models.SkinLog{
		UserID: user.ID,
		Dif:    1,
	})
	if err != nil {
		return err
	}

	inven := user.Inventory
	inven.TwoColorSkin++
	err = p.DB.Update(&inven)
	if err != nil {
		return err
	}

	_, err = p.Redis.Conn().Do("DEL", redisOrder(orderid))
	if err != nil {
		return err
	}

	return nil
}

func (p *PayServ) createOrder(orderid int, userid int) error {
	_, err := p.Redis.Conn().Do("SET", redisOrder(orderid), userid)
	if err != nil {
		return err
	}
	return err
}
