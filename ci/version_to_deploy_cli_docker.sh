#!/bin/bash

echo checking init container version
CLI_VERSION=$(grep -E "\"version\"" ./cli/package.json | grep -Eo "[0-9.]*(-rc[0-9]*)?")
CLI_TAG="cli-docker-$CLI_VERSION"
export CLI_DOCKER_TAG="latest"
if [[ "$(git tag | grep -c "$CLI_TAG")" == "0" ]]; then
    echo tagging "$CLI_TAG"
    git tag "$CLI_TAG"
    export CLI_DOCKER_TAG=$CLI_VERSION
fi

echo "export CLI_DOCKER_TAG=$CLI_DOCKER_TAG"  >>  "$BASH_ENV"