#!/usr/bin/env bash

set -e

api_file="docker-compose.ci.yaml"

if [[ -z $IMAGE_TAG ]];
then
    api_file="docker-compose.local.yaml"
fi

docker-compose -f tests/blackbox/compose/docker-compose.yaml -f tests/blackbox/compose/$api_file -f tests/blackbox/compose/docker-compose.security.yaml pull --parallel
docker-compose -f tests/blackbox/compose/docker-compose.yaml -f tests/blackbox/compose/$api_file -f tests/blackbox/compose/docker-compose.security.yaml up --build --exit-code-from black-box --abort-on-container-exit