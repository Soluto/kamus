#!/usr/bin/env bash

set -o errexit
set -o nounset
set -o pipefail

readonly KIND_VERSION=0.2.1
readonly CLUSTER_NAME=e2e-test
readonly K8S_VERSION=v1.13.2
readonly KUBECTL_VERSION=v1.13.0

run_e2e_container() {
    echo 'Running e2e container...'
    docker run --rm --interactive --detach --network host --name e2e \
        --volume "$(pwd):/workdir" \
        --workdir /workdir/tests/crd-controller \
        "microsoft/dotnet:2.2-sdk" \
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
    echo 'kubernetes version' $K8S_VERSION

    curl -sSLo kind "https://github.com/kubernetes-sigs/kind/releases/download/$KIND_VERSION/kind-linux-amd64"
    chmod +x kind
    sudo mv kind /usr/local/bin/kind

    curl -sSLO https://storage.googleapis.com/kubernetes-release/release/$KUBECTL_VERSION/bin/linux/amd64/kubectl
    chmod +x kubectl

    docker cp kubectl e2e:/usr/local/bin/kubectl

    kind create cluster --name "$CLUSTER_NAME" --config tests/crd-controller/kind-config.yaml --image "kindest/node:$K8S_VERSION"

    kind load image-archive docker-cache-api/crd-controller.tar --name "$CLUSTER_NAME"
    docker_exec mkdir -p /root/.kube

    echo 'Copying kubeconfig to container...'
    local kubeconfig
    kubeconfig="$(kind get kubeconfig-path --name "$CLUSTER_NAME")"
    docker cp "$kubeconfig" e2e:/root/.kube/config

    docker_exec kubectl cluster-info
    echo

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

    create_kind_cluster $1

    run_test
}

main