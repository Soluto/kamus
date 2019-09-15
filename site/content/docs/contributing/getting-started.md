---
title: "Getting Started"
menu:
  main:
    parent: "contributing"
    identifier: "getting started"
    weight: 1
---
# Getting Started

Welcome! This guide covers how to get started contributing to Kamus.

## 1. Install Tools

### Install git

Our source code is managed with [`git`][git], to develop locally you
will need to install `git`.

You can check if `git` is already on your system and properly installed with 
the following command:

```
git --version
```

### Install Hugo

If you wish to contribute to the documentation, it is recommended but not 
required to install [hugo], which we use to develop this site.

Please see: https://gohugo.io/getting-started/installing/


### Install .NET core

To work on Kamus's API codebase you will need [.NET core].

Install or upgrade [.NET core using the instructions for your operating system][.NET core].
You can check if .NET is in your system with the following command:

```
dotnet --version
```

Required .NET `2.2.103` or greater should be installed. 

### Install NodeJS

To work on Kamus's CLI or init container you will need [NodeJS]

Install or upgrade [NodeJS using the instructions for your operating system][NodeJS].
You can check if NodeJS is in your system with the following command:

```
node --version
```

Required NodeJS `v10.0.0` or greater should be installed. 

It is recommended to use [yarn] instead of NPM. 
You can check if Yarn is in your system using the following command:

```
yarn --version
```

Recommended Yarn `1.12.1` or greater should be installed. 

### Install Docker

To develop Kamus you will need to install [Docker][docker].

If you haven't already, [install Docker][install docker], following the
[official instructions][install docker].
If you have an existing installation, check your version and make sure you have
the latest Docker.

To check if `docker` is has been installed:
```
docker --version
```
Recommend usin Docker version 18.09.2 or greater.

### Install Kubernetes

You'll also need [Kubernetes] to develop Kamus.

To install Kubernetes locally, you can use either [minikube], [docker for desktop] or [kind] (which is also required for running the black box tests).
To interact with the cluster you'll also have to install [kubectl]

You can check if NodeJS is in your system with the following command:

```
kubectl version
```

Kamus support all the versions supported by Kubernetes - currently it.s 1.13.\*, 1.14.\* or 1.15.\*.

## 2. Read The Docs 

The [roadmap], [architecture], and [threat modeling]
may be helpful to review before contributing.

## 3. Running the CRD controller locally
In order to run the CRD controller locally, you need to a TLS certificate that will be used by the controller.
There are 2 options to achieve that:

* Use the [test deployment files](https://github.com/Soluto/kamus/blob/master/tests/crd-controller/deployment.yaml) to deploy the CRD controller to a cluster. The file contain all the is required for running it. Don't forget also to deploy the [CRD definition](https://github.com/Soluto/kamus/blob/master/tests/crd-controller/crd.yaml) to the same cluster.

* Running locally by generating a certificate and private key using:
```
openssl req -x509 -sha256 -nodes -days 365 -newkey rsa:2048 \
     -keyout privateKey.key -out certificate.crt \
    -subj "/CN=kamus-crd.default.svc"
```
After doing that, don't forget to set `TLS_CERT_FOLDER` to the folder where the files exist. 

## 4. Reaching Out

Issues are tracked on GitHub. Please check [the issue tracker][issues] to see
if there is any existing dicussion or work related to your interests.

If you do not see anything, please [file a new issue][file an issue].

Please reach out for bugs, feature requests, and other issues!  
The maintainers of this project are reachable via:

- reach out and ask for help on the [kamus slack][kamus slack] (use the [slack invite] link)
- [filing an issue][file an issue]



Current maintainers are [@omerlh] and [@shaikatz] - feel free to
reach out if you have any questions!

[git]: https://git-scm.com/
[hugo]: https://gohugo.io
[roadmap]: /docs/contributing/roadmap
[architecture]: /docs/threatmodeling/architecture
[threat modeling]: /docs/threatmodeling/threats_controls
[github]: https://github.com/
[.NET core]: https://dotnet.microsoft.com/download
[NodeJS]: https://nodejs.org/en/download/
[yarn]: https://yarnpkg.com/lang/en/docs/install/
[docker]: https://www.docker.com/
[install docker]: https://docs.docker.com/install/#supported-platforms
[Kubernetes]: https://kubernetes.io
[minikube]: https://kubernetes.io/docs/tasks/tools/install-minikube/
[docker for desktop]: https://docs.docker.com/docker-for-mac/#kubernetes
[kubectl]: https://kubernetes.io/docs/tasks/tools/install-kubectl/
[community]: https://github.com/kubernetes/community
[contributor]: https://github.com/kubernetes/community/blob/master/contributors/guide/README.md
[issues]: https://github.com/Soluto/Kamus/issues
[file an issue]: https://github.com/Soluto/Kamus/issues/new
[kamus slack]: http://k8s-kamus.slack.io/
[slack invite]: https://join.slack.com/t/k8s-kamus/shared_invite/enQtNTQwMjc2MzIxMTM3LTgyYTcwMTUxZjJhN2JiMTljMjNmOTBmYjEyNWNmZTRiNjVhNTUyYjMwZDQ0YWQ3Y2FmMTBlODA5MzFlYjYyNWE
[@omerlh]: https://github.com/omerlh
[@shaikatz]: https://github.com/shaikatz