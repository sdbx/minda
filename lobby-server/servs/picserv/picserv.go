package picserv

import (
	"bytes"
	"fmt"
	"image"
	_ "image/gif"
	_ "image/jpeg"
	"image/png"
	"lobby/models"
	"lobby/servs/dbserv"
	"lobby/servs/redisserv"
	"net/http"

	"github.com/garyburd/redigo/redis"
)

const (
	redisCoolTmpl = "pic_cool_%d"
	coolTime      = 10
)

func redisCool(id int) string {
	return fmt.Sprintf(redisCoolTmpl, id)
}

type PicServ struct {
	Redis *redisserv.RedisServ `dim:"on"`
	DB    *dbserv.DBServ       `dim:"on"`
}

func Provide() *PicServ {
	return &PicServ{}
}

func (p *PicServ) DownloadImage(url string) (image.Image, error) {
	fmt.Println(url)
	resp, err := http.Get(url)
	if err != nil {
		return nil, err
	}
	defer resp.Body.Close()

	img, _, err := image.Decode(resp.Body)
	if err != nil {
		return nil, err
	}
	return img, nil
}

func (p *PicServ) UploadBuffer(buf []byte) (int, error) {
	img, _, err := image.Decode(bytes.NewReader(buf))
	if err != nil {
		return 0, err
	}
	return p.UploadImage(img)
}

func (p *PicServ) UploadImage(img image.Image) (int, error) {
	var buf bytes.Buffer
	err := png.Encode(&buf, img)
	if err != nil {
		return 0, err
	}
	item := models.Picture{
		Payload: buf.Bytes(),
	}
	err = p.DB.Create(&item)
	if err != nil {
		return 0, err
	}
	return item.ID, nil
}

func (p *PicServ) GetImage(id int) ([]byte, error) {
	var item models.Picture
	err := p.DB.Q().Where("id = ?", id).First(&item)
	if err != nil {
		return nil, err
	}

	return item.Payload, nil
}

func (p *PicServ) SetCool(id int) error {
	_, err := p.Redis.Conn().Do("SET", redisCool(id), 1)
	if err != nil {
		return err
	}
	_, err = p.Redis.Conn().Do("EXPIRE", redisCool(id), coolTime)
	return err
}

func (p *PicServ) IsCool(id int) (bool, error) {
	return redis.Bool(p.Redis.Conn().Do("EXISTS", redisCool(id)))
}
