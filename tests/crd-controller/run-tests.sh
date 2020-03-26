#!/usr/bin/env bash

set -o errexit
set -o nounset
set -o pipefail

readonly KIND_VERSION=0.7.0
readonly CLUSTER_NAME=e2e-test
readonly KUBECTL_VERSION=v1.13.0

run_e2e_container() {
    echo 'Running e2e container...'
    docker run --rm --interactive --detach --network host --name e2e \
        --volume "$(pwd):/workdir" \
        --workdir /workdir/tests/crd-controller \
        "mcr.microsoft.com/dotnet/core/sdk:3.0" \
        cat
    echo
}

cleanup() {
    echo 'Removing ct container...'
    docker kill e2e > /dev/null 2>&1

    echo 'Done!'
}

docker_exec() {
    docker exec --interactive e2e "$@"
}

create_kind_cluster() {
    K8S_VERSION=$1
    echo 'Installing kind...'
    echo 'kubernetes version' "$K8S_VERSION"

    curl -sfSLo kind "https://github.com/kubernetes-sigs/kind/releases/download/v$KIND_VERSION/kind-linux-amd64"
    chmod +x kind
    sudo mv kind /usr/local/bin/kind

    curl -sfSLO https://storage.googleapis.com/kubernetes-release/release/$KUBECTL_VERSION/bin/linux/amd64/kubectl
    chmod +x kubectl

    docker cp kubectl e2e:/usr/local/bin/kubectl

    kind_config="kind-config.yaml"

    TMPDIR=$HOME kind create cluster --name "$CLUSTER_NAME" --config tests/crd-controller/$kind_config --image "kindest/node:$K8S_VERSION"

    kind load image-archive docker-cache-api/crd-controller.tar --name "$CLUSTER_NAME"
    docker_exec mkdir -p /root/.kube

    echo 'Copying kubeconfig to container...'
    docker cp ~/.kube/config e2e:/root/.kube/config

    echo -n 'Waiting for cluster to be ready...'
    until ! grep --quiet 'NotReady' <(docker_exec kubectl get nodes --no-headers); do
        printf '.'
        sleep 1
    done

    echo '✔︎'
    echo

    docker_exec kubectl get nodes
    echo

    echo 'Cluster ready!'
    echo
}

run_test() {
    docker_exec dotnet test 
    echo
}

main() {
    run_e2e_container
    trap cleanup EXIT

    create_kind_cluster "$1"

    run_test
}

main "$@"