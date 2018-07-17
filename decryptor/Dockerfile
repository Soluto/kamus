FROM golang:1.8 AS build

WORKDIR /usr/local/go/src/decryptor

COPY main.go .

RUN CGO_ENABLED=0 go build -o /usr/local/go/src/decryptor/decryptor decryptor && cp /usr/local/go/src/decryptor/decryptor /usr/local/bin

CMD decryptor