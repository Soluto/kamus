#!/bin/bash

echo checking decryptor api version
DECRYPTOR_API_VERSION=$(grep -E "<Version>" ./src/decrypt-api/decrypt-api.csproj | grep -Eo "[0-9.]*(-rc[0-9]*)?")
DECRYPTOR_API_TAG="decryptor-$DECRYPTOR_API_VERSION"
export DECRYPTOR_API_DOCKER_TAG="decryptor-latest"
if [[ "$(git tag | grep -c "$DECRYPTOR_API_TAG")" == "0" ]]; then
    echo tagging "$DECRYPTOR_API_TAG"
    git tag "$DECRYPTOR_API_TAG"
    export DECRYPTOR_API_DOCKER_TAG=$DECRYPTOR_API_TAG
fi

echo checking encryptor api version
ENCRYPTOR_API_VERSION=$(grep -E "<Version>" ./src/encrypt-api/encrypt-api.csproj | grep -Eo "[0-9.]*(-rc[0-9]*)?")
ENCRYPTOR_API_TAG="encryptor-$ENCRYPTOR_API_VERSION"
export ENCRYPTOR_API_DOCKER_TAG="encryptor-latest"
if [[ "$(git tag | grep -c "$ENCRYPTOR_API_TAG")" == "0" ]]; then
    echo tagging "$ENCRYPTOR_API_TAG"
    git tag "$ENCRYPTOR_API_TAG"
    export ENCRYPTOR_API_DOCKER_TAG=$ENCRYPTOR_API_TAG
fi

echo checking controller api version
CONTROLLER_API_VERSION=$(grep -E "<Version>" ./src/crd-controller/crd-controller.csproj | grep -Eo "[0-9.]*(-rc[0-9]*)?")
CONTROLLER_API_TAG="controller-$CONTROLLER_API_VERSION"
export CONTROLLER_API_DOCKER_TAG="controller-latest"
if [[ "$(git tag | grep -c "$CONTROLLER_API_TAG")" == "0" ]]; then
    echo tagging "$CONTROLLER_API_TAG"
    git tag "$CONTROLLER_API_TAG"
    export CONTROLLER_API_DOCKER_TAG=$CONTROLLER_API_TAG
fi

echo  checking cli version
CLI_VERSION=$(grep -E "\"version\"" ./cli/package.json  | grep -Eo "[0-9.]*(-rc[0-9]*)?")
CLI_TAG="cli-$CLI_VERSION"
export TWEEK_DOCKER_TAG_EDITOR="latest"
if [[ "$(git tag | grep -c "$CLI_TAG")" == "0" ]]; then
    echo tagging "$CLI_TAG"
    git tag "$CLI_TAG"
    export TWEEK_DOCKER_TAG_EDITOR=$CLI_VERSION
fi

{
    echo "export DECRYPTOR_API_DOCKER_TAG=$DECRYPTOR_API_DOCKER_TAG"
    echo "export ENCRYPTOR_API_DOCKER_TAG=$ENCRYPTOR_API_DOCKER_TAG"
} >> "$BASH_ENV"