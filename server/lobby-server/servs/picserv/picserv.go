package picserv

import (
	"bytes"
	"fmt"
	"image"
	"image/color"
	_ "image/gif"
	_ "image/jpeg"
	"image/png"
	"io"
	"lobby/servs/redisserv"
	"lobby/utils"
	"mime/multipart"
	"strconv"
	"time"

	"github.com/garyburd/redigo/redis"
	"github.com/minio/minio-go"
)

const (
	skinSize      = 128
	skinBorder    = 0.08
	profileSize   = 128
	redisCoolTmpl = "pic_cool_%d"
	coolTime      = 10
	maxSize       = 1000
	minSize       = 50
)

var (
	ColorBlack = color.RGBA{0, 0, 0, 255}
	ColorWhite = color.RGBA{255, 255, 255, 255}
)

func redisCool(id int) string {
	return fmt.Sprintf(redisCoolTmpl, id)
}

type PicServConf struct {
	Endpoint string `yaml:"endpoint"`
	Key      string `yaml:"key"`
	Secret   string `yaml:"secret"`
	Region   string `yaml:"region"`
	Bucket   string `yaml:"bucket"`
	ExternalName string `yaml:"external_name"`
}

type PicServ struct {
	Redis    *redisserv.RedisServ `dim:"on"`
	bucket   string
	endpoint string
	external string
	cli      *minio.Client
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
	policy := `{"Version": "2012-10-17","Statement": [{"Action": ["s3:GetObject"],"Effect": "Allow","Principal": {"AWS": ["*"]},"Resource": ["arn:aws:s3:::` + conf.Bucket + `/*"],"Sid": ""}]}`
	err = cli.SetBucketPolicy(conf.Bucket, policy)
	if err != nil {
		return nil, err
	}
	return &PicServ{
		cli:      cli,
		endpoint: conf.Endpoint,
		bucket:   conf.Bucket,
		external: conf.ExternalName,
	}, err
}

func (p PicServ) ConfigName() string {
	return "pic"
}

func (p *PicServ) ParseImage(r io.Reader) (image.Image, error) {
	img, _, err := image.Decode(r)
	return img, err
}

func (p *PicServ) ParseImageFromFile(f *multipart.FileHeader) (image.Image, error) {
	r, err := f.Open()
	if err != nil {
		return nil, err
	}
	defer r.Close()
	return p.ParseImage(r)
}

func (p *PicServ) UploadImage(img image.Image) (string, error) {
	var buf bytes.Buffer
	err := png.Encode(&buf, img)
	if err != nil {
		return "", err
	}
	key := strconv.Itoa(int(time.Now().UnixNano())) + utils.RandString(30) + ".png"
	_, err = p.cli.PutObject(p.bucket, key, &buf, -1, minio.PutObjectOptions{ContentType: "image/png"})
	if err != nil {
		return "", err
	}
	if p.external != "" {
		return "http://" + p.external + "/" + p.bucket + "/" + key, nil
	}
	return "http://" + p.bucket + "." + p.endpoint + "/" + key, nil
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
