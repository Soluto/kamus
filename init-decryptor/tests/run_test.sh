#!/usr/bin/env bash

set -e

echo "starting wiremock"

# docker-compose up -d --build wiremock

docker-compose build decryptor

echo "running decryptor"

docker-compose run decryptor

echo "comparing files"

diff -q out.json expected.json