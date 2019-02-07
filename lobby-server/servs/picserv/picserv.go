package picserv

import (
	"time"
	"lobby/utils"
	"bytes"
	"fmt"
	"image"
	_ "image/gif"
	_ "image/jpeg"
	"image/png"
	"lobby/servs/redisserv"
	"net/http"

	"github.com/minio/minio-go"
	"github.com/garyburd/redigo/redis"
)

const (
	redisCoolTmpl = "pic_cool_%d"
	coolTime      = 10
)

func redisCool(id int) string {
	return fmt.Sprintf(redisCoolTmpl, id)
}

type PicServConf struct {
	Endpoint string `yaml:"endpoint"`
	Key string `yaml:"key"`
	Secret string `yaml:"key"`
	Region string `yaml:"region"`
	Bucket string `yaml:"bucket"`
}

type PicServ struct {
	Redis *redisserv.RedisServ `dim:"on"`
	bucket string
	endpoint string
	cli *minio.Client
}

func Provide(conf PicServConf) (*PicServ, error) {
	cli, err := minio.New(conf.Endpoint, conf.Key, conf.Secret, false)
	if err != nil {
		return nil, err
	}
	err = cli.MakeBucket(conf.Bucket, conf.Region)
    if err != nil {
        exists, err := cli.BucketExists(conf.Bucket)
        if err != nil || !exists {
			return nil, err
        }
	}
	policy := `{"Version": "2012-10-17","Statement": [{"Action": ["s3:GetObject"],"Effect": "Allow","Principal": {"AWS": ["*"]},"Resource": ["arn:aws:s3:::`+ conf.Bucket + `/*"],"Sid": ""}]}`
	err = cli.SetBucketPolicy(conf.Bucket, policy)
	if err != nil {
		return nil, err
	}
	return &PicServ {
		cli: cli,
		endpoint: conf.Endpoint,
		bucket: conf.Bucket,
	}, err
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

func (p *PicServ) UploadBuffer(buf []byte) (string, error) {
	img, _, err := image.Decode(bytes.NewReader(buf))
	if err != nil {
		return "", err
	}
	return p.UploadImage(img)
}

func (p *PicServ) UploadImage(img image.Image) (string, error) {
	var buf bytes.Buffer
	err := png.Encode(&buf, img)
	if err != nil {
		return "", err
	}
	key := time.Now().String()+utils.RandString(30)+".png"
	_, err = p.cli.PutObject(p.bucket, key, &buf, -1, minio.PutObjectOptions{ContentType:"image/png"})
	if err != nil {
		return "", err
	}

	return "http://" + p.endpoint + "/" + key, nil
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
