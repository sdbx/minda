FROM golang:1.12

RUN go get -u github.com/gobuffalo/packr/v2/packr2

WORKDIR /minda

COPY go.mod .
COPY go.sum .

RUN go mod download

COPY . .

RUN GO111MODULE=on packr2

RUN GO111MODULE=on go install

WORKDIR /home

CMD ["lobby"]