---
title: "Known Issues"
menu:
  main:
    parent: "user"
    identifier: "known-issues"
    weight: 3
---
# Known Issues

Having problems with kind? This guide is covers some known problems and solutions / workarounds.

It may additionally be helpful to:

- check our [issue tracker]
- [file an issue][file an issue] (if there isn't one already)
- reach out and ask for help in [#kind] on the [kubernetes slack]

## Contents
* [Docker on Btrfs](#docker-on-btrfs)
* [Failing to apply overlay network](#failing-to-apply-overlay-network)
* [Failure to build node image](#failure-to-build-node-image)
* [Failure for cluster to properly start](#failure-for-cluster-to-properly-start)

## Docker on Btrfs

`kind` cannot run properly if containers on your machine / host are backed by a
[Btrfs](https://en.wikipedia.org/wiki/Btrfs) filesystem.

This should only be relevant on linux, on which you can check with:

`stat --file-system --format=%T $(docker info --format {{.DockerRootDir}})`

To fix this you must ensure that your containers are not backed by Btrfs, there
is no other known workaround at this time.

## Failing to apply overlay network
There are two known causes for problems while applying the overlay network
while building a kind cluster:

* Host machine is behind a proxy
* Usage of Docker version 18.09
* Building kind in Google Cloud Console

If you see something like the following error message:
```
 ✗ [kind-1-control-plane] Starting Kubernetes (this may take a minute) ☸
FATA[07:20:43] Failed to create cluster: failed to apply overlay network: exit status 1
```

or the following, when setting the `loglevel` flag to debug,
```
DEBU[16:26:53] Running: /usr/bin/docker [docker exec --privileged kind-1-control-plane /bin/sh -c kubectl apply --kubeconfig=/etc/kubernetes/admin.conf -f "https://cloud.weave.works/k8s/net?k8s-version=$(kubectl version --kubeconfig=/etc/kubernetes/admin.conf | base64 | tr -d '\n')"]
ERRO[16:28:25] failed to apply overlay network: exit status 1 ) ☸
 ✗ [control-plane] Starting Kubernetes (this may take a minute) ☸
ERRO[16:28:25] failed to apply overlay network: exit status 1
DEBU[16:28:25] Running: /usr/bin/docker [docker ps -q -a --no-trunc --filter label=io.k8s.sigs.kind.cluster --format {{.Names}}\t{{.Label "io.k8s.sigs.kind.cluster"}} --filter label=io.k8s.sigs.kind.cluster=1]
DEBU[16:28:25] Running: /usr/bin/docker [docker rm -f -v kind-1-control-plane]
⠈⠁ [control-plane] Pre-loading images 🐋 Error: failed to create cluster: failed to apply overlay network: exit status 1
```

The issue may be due to your host machine being behind a proxy, such as in
[kind#136][kind#136].
We are currently looking into ways of mitigating this issue by preloading CNI
artifacts, see [kind#200][kind#200].
Another possible solution is to enable kind nodes to use a proxy when
downloading images, see [kind#270][kind#270].

A similar issue has been observed when using 
[kind in Google Cloud Console in kind#182][kind#182].
More information is needed in this case but it is believed that the issue is
due to Google Cloud Console being behind a proxy.

The last known case for this issue comes from the host machine 
[using Docker 18.09 in kind#136][kind#136-docker]. 
In this case, a known solution is to upgrade to any Docker version greater than or
equal to Docker 18.09.1.


## Failure to build node image
The know case in which building kind's node image may fail is due to 
Docker on Mac running out of memory, see [kind#229][kind#229].
If you see something like this:
```
    cmd/kube-scheduler
    cmd/kube-proxy
/usr/local/go/pkg/tool/linux_amd64/link: signal: killed
!!! [0116 08:30:53] Call tree:
!!! [0116 08:30:53]  1: /go/src/k8s.io/kubernetes/hack/lib/golang.sh:614 kube::golang::build_some_binaries(...)
!!! [0116 08:30:53]  2: /go/src/k8s.io/kubernetes/hack/lib/golang.sh:758 kube::golang::build_binaries_for_platform(...)
!!! [0116 08:30:53]  3: hack/make-rules/build.sh:27 kube::golang::build_binaries(...)
!!! [0116 08:30:53] Call tree:
!!! [0116 08:30:53]  1: hack/make-rules/build.sh:27 kube::golang::build_binaries(...)
!!! [0116 08:30:53] Call tree:
!!! [0116 08:30:53]  1: hack/make-rules/build.sh:27 kube::golang::build_binaries(...)
make: *** [all] Error 1
Makefile:92: recipe for target 'all' failed
!!! [0116 08:30:54] Call tree:
!!! [0116 08:30:54]  1: build/../build/common.sh:518 kube::build::run_build_command_ex(...)
!!! [0116 08:30:54]  2: build/release-images.sh:38 kube::build::run_build_command(...)
make: *** [quick-release-images] Error 1
ERRO[08:30:54] Failed to build Kubernetes: failed to build images: exit status 2
Error: error building node image: failed to build kubernetes: failed to build images: exit status 2
Usage:
  kind build node-image [flags]

Flags:
      --base-image string   name:tag of the base image to use for the build (default "kindest/base:v20181203-d055041")
  -h, --help                help for node-image
      --image string        name:tag of the resulting image to be built (default "kindest/node:latest")
      --kube-root string    Path to the Kubernetes source directory (if empty, the path is autodetected)
      --type string         build type, one of [bazel, docker, apt] (default "docker")

Global Flags:
      --loglevel string   logrus log level [panic, fatal, error, warning, info, debug] (default "warning")

error building node image: failed to build kubernetes: failed to build images: exit status 2
```

Then you may try increasing the resource limits for the Docker engine on Mac.

It is recommended that you allocate at least 8GB of RAM to build Kubernetes.

Open the **Preferences** menu.

<img src="/docs/user/images/docker-pref-1.png"/>

Go to the **Advanced** settings page, and change the settings there, see 
[changing Docker's resource limits][Docker resource lims].

<img width="400px" src="/docs/user/images/docker-pref-build.png"/>


## Failing to properly start cluster
This issue is similar to a 
[failure while building the node image](#failure-to-build-node-image).
If the cluster creation process was successul but you are unable to see any
Kubernetes resources running, for example:

```
$ docker ps
CONTAINER ID        IMAGE                  COMMAND                  CREATED              STATUS              PORTS                      NAMES
c0261f7512fd        kindest/node:v1.12.2   "/usr/local/bin/entr…"   About a minute ago   Up About a minute   0.0.0.0:64907->64907/tcp   kind-1-control-plane
$ docker exec -it c0261f7512fd /bin/sh
# docker ps -a
CONTAINER ID        IMAGE               COMMAND             CREATED             STATUS              PORTS               NAMES
#
```

or `kubectl` being unable to connect to the cluster,
```
$ export KUBECONFIG="$(kind get kubeconfig-path)"
$ kubectl cluster-info

To further debug and diagnose cluster problems, use 'kubectl cluster-info dump'.
Unable to connect to the server: EOF
```

Then as in [kind#156][kind#156], you may solve this issue by claiming back some
space on your machine by removing unused data or images left by the Docker 
engine by running:
```
docker system prune
```

and / or:
```
docker image prune
```

You can verify the issue by exporting the logs (`kind export logs`) and looking
at the kubelet logs, which may have something like the following:
```
Dec 07 00:37:53 kind-1-control-plane kubelet[688]: I1207 00:37:53.229561     688 eviction_manager.go:340] eviction manager: must evict pod(s) to reclaim ephemeral-storage
Dec 07 00:37:53 kind-1-control-plane kubelet[688]: E1207 00:37:53.229638     688 eviction_manager.go:351] eviction manager: eviction thresholds have been met, but no pods are active to evict
```

If on the other hand you are running kind on a btrfs partition and your logs
show something like the following:
```
an 03 17:42:41 kind-1-control-plane kubelet[3804]: F0103 17:42:41.470269 3804 kubelet.go:1359] Failed to start ContainerManager failed to get rootfs info: failed to get device for dir "/ var/lib/kubelet": could not find device with major: 0, minor: 67 in cached partitions map
```

This problem seems to be related to a [bug in Docker][moby#9939].

[issue tracker]: https://github.com/kubernetes-sigs/kind/issues
[file an issue]: https://github.com/kubernetes-sigs/kind/issues/new
[#kind]: https://kubernetes.slack.com/messages/CEKK1KTN2/
[kubernetes slack]: http://slack.k8s.io/
[kind#136]: https://github.com/kubernetes-sigs/kind/issues/136
[kind#136-docker]: https://github.com/kubernetes-sigs/kind/issues/136#issuecomment-457015838
[kind#156]: https://github.com/kubernetes-sigs/kind/issues/156
[kind#182]: https://github.com/kubernetes-sigs/kind/issues/182
[kind#200]: https://github.com/kubernetes-sigs/kind/issues/200
[kind#229]: https://github.com/kubernetes-sigs/kind/issues/229
[kind#270]: https://github.com/kubernetes-sigs/kind/issues/270
[moby#9939]: https://github.com/moby/moby/issues/9939
[Docker resource lims]: https://docs.docker.com/docker-for-mac/#advanced
