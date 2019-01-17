echo checking init container version
INIT_CONTAINER_VERSION=$(grep -E "\"version\"" ./init-container/package.json | grep -Eo "[0-9.]*(-rc[0-9]*)?")
INIT_CONTAINER_TAG="init-container-$INIT_CONTAINER_VERSION"
export INIT_CONTAINER_DOCKER_TAG="latest"
if [[ "$(git tag | grep -c "$INIT_CONTAINER_TAG")" == "0" ]]; then
    echo tagging "$INIT_CONTAINER_TAG"
    git tag "$INIT_CONTAINER_TAG"
    export INIT_CONTAINER_DOCKER_TAG=$INIT_CONTAINER_VERSION
fi

echo "export INIT_CONTAINER_DOCKER_TAG=$INIT_CONTAINER_DOCKER_TAG"  >>  "$BASH_ENV"