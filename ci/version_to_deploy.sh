echo checking decryptor api version
DECRYPTOR_API_VERSION=$(cat ./src/decrypt-api/decrypt-api.csproj | grep -E "<Version>" | grep -Eo "[0-9.]*(-rc[0-9]*)?")
DECRYPTOR_API_TAG="decryptor-$DECRYPTOR_API_VERSION"
export DECRYPTOR_API_DOCKER_TAG="decryptor-latest"
if [[ "$(git tag | grep -c $DECRYPTOR_API_TAG)" == "0" ]]; then
    echo tagging $DECRYPTOR_API_TAG
    git tag $DECRYPTOR_API_TAG
    export DECRYPTOR_API_DOCKER_TAG=$DECRYPTOR_API_VERSION
fi

echo checking encryptor api version
ENCRYPTOR_API_VERSION=$(cat ./src/encrypt-api/encrypt-api.csproj | grep -E "<Version>" | grep -Eo "[0-9.]*(-rc[0-9]*)?")
ENCRYPTOR_API_TAG="encryptor-$ENCRYPTOR_API_VERSION"
export ENCRYPTOR_API_DOCKER_TAG="encryptor-latest"
if [[ "$(git tag | grep -c $ENCRYPTOR_API_TAG)" == "0" ]]; then
    echo tagging $ENCRYPTOR_API_TAG
    git tag $ENCRYPTOR_API_TAG
    export ENCRYPTOR_API_DOCKER_TAG=$ENCRYPTOR_API_VERSION
fi

echo checking init container version
INIT_CONTAINER_VERSION=$(cat ./init-container/package.json | grep -E "\"version\"" | grep -Eo "[0-9.]*(-rc[0-9]*)?")
INIT_CONTAINER_TAG="init-container-$INIT_CONTAINER_VERSION"
export TWEEK_DOCKER_TAG_AUTHORING="latest"
if [[ "$(git tag | grep -c $INIT_CONTAINER_TAG)" == "0" ]]; then
    echo tagging $INIT_CONTAINER_TAG
    git tag $INIT_CONTAINER_TAG
    export TWEEK_DOCKER_TAG_AUTHORING=$INIT_CONTAINER_VERSION
fi

echo  checking cli version
CLI_VERSION=$(cat ./cli/package.json | grep -E "\"version\"" | grep -Eo "[0-9.]*(-rc[0-9]*)?")
CLI_TAG="cli-$CLI_VERSION"
export TWEEK_DOCKER_TAG_EDITOR="latest"
if [[ "$(git tag | grep -c $CLI_TAG)" == "0" ]]; then
    echo tagging $CLI_TAG
    git tag $CLI_TAG
    export TWEEK_DOCKER_TAG_EDITOR=$CLI_VERSION
fi