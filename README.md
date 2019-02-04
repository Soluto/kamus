[![Helm Package](https://img.shields.io/badge/helm-latest-blue.svg)](https://hub.helm.sh/charts/soluto/kamus) 
[![Docker](https://img.shields.io/badge/dockerhub-latest-blue.svg)](https://hub.docker.com/r/soluto/kamus)
[![Slack](https://img.shields.io/badge/slack-kamus-orange.svg)](https://join.slack.com/t/k8s-kamus/shared_invite/enQtNTQwMjc2MzIxMTM3LTgyYTcwMTUxZjJhN2JiMTljMjNmOTBmYjEyNWNmZTRiNjVhNTUyYjMwZDQ0YWQ3Y2FmMTBlODA5MzFlYjYyNWE)
[![Twitter](https://img.shields.io/twitter/follow/solutoeng.svg?label=Follow&style=popout)](https://twitter.com/intent/tweet?text=Checkout%20Kamus%20secret%20encryption%20for%20Kubernetes&url=https://github.com/Soluto/kamus&via=SolutoEng&hashtags=kubernetes,devops,devsecops) [![CircleCI](https://circleci.com/gh/Soluto/kamus.svg?style=svg)](https://circleci.com/gh/Soluto/kamus)
# Kamus ![logo](images/logo.png)  
An open source, GitOps, zero-trust secrets encryption and decryption solution for Kubernetes applications.
Kamus enable users to easily encrypt secrets than can be decrypted only by the application running on Kubernetes.
The encryption is done using strong encryption providers (currently supported: Azure KeyVault, Google Cloud KMS and AES).
To learn more about Kamus, check out the [blog post](https://blog.solutotlv.com/can-kubernetes-keep-a-secret?utm_source=github) and [slides](https://www.slideshare.net/SolutoTLV/can-kubernetes-keep-a-secret).
## Getting Started

The simple way to run Kamus is by using the Helm chart:
```
helm repo add soluto https://charts.soluto.io
helm upgrade --install kamus soluto/kamus
```
Refer to the [installation guide](./docs/install.md) for more details.
After installing Kamus, you can start using it to encrypt secrets.
Kamus encrypt secrets for a specific application, represent by a [Kubernetes Service Account](https://kubernetes.io/docs/tasks/configure-pod-container/configure-service-account).
Create a service account for your application, and mount it on the pods running your application.
Now, when you know the name of the service account, and the namespace it exists in, install Kamus CLI:
```
npm install -g @soluto-asurion/kamus-cli
```
Use Kamus CLI to encrypt the secret:
```
kamus-cli encrypt --secret super-secret --service-account kamus-example-sa --namespace default --kamus-url <Kamus URL>
```
*If you're running Kamus locally the Kamus URL will be like `http://localhost:<port>`. So you need to add `--allow-insecure-url` flag to enable http protocol.*

Pass the value returned by the CLI to your pod, and use Kamus Decrypt API to decrypt the value.
The simplest way to achieve that is by using the init container.
An alternative is to use Kamus decrypt API directly in the application code.
To make it clearer, take a look on a working [example app](example/README.md).
You can deploy this app to any Kubernetes cluster that has Kamus installed, to understand how it works.

Have a question? Something is not clear? Reach out to us on [Kamus Slack](https://join.slack.com/t/k8s-kamus/shared_invite/enQtNTQwMjc2MzIxMTM3LTgyYTcwMTUxZjJhN2JiMTljMjNmOTBmYjEyNWNmZTRiNjVhNTUyYjMwZDQ0YWQ3Y2FmMTBlODA5MzFlYjYyNWE)!

## Architecture
Kamus has 3 components:
* Encrypt API
* Decrypt API
* Key Management System (KMS)

The encrypt and decrypt APIs handle encryption and decryption requests.
The KMS is a wrapper for various cryptographic solutions. Currently supported:
* AES - uses one key for all secrets
* Azure KeyVault - creates one key per service account.
* Google Cloud KMS - creates one key per service account.

We look forward to add support for other cloud solutions, like AWS KMS.
If you're interested in such a feature, please let us know.
We would like help with testing it out.
Consult the [installation guide](docs/install.md) for more details on how to deploy Kamus using the relevant KMS.

### Utilities
Kamus is shipped with 2 utilities that make it easier to use:
* Kamus CLI - a small CLI that eases the interaction with the Encrypt API. Refer to the docs for more details.
* Kamus init container - a init container that interacts with the Decrypt API. Refer to the docs for more details.

## Security
We take security seriously at Soluto.
To learn more about the security aspects of Kamus refer to the Threat Modeling docs containing all the various threats and mitigations we discussed.
Before installing Kamus in production refer the installation guide to learn the best practices of deploying Kamus securely.
In case you find a security issue or have something you would like to discuss refer to our [security.md](security.md) policy.

## Contributing
Find a bug? Have a missing feature? Please open an issue and let us know.
We would like to help you using Kamus!
Please notice: Do not report security issues on GitHub.
We will immediately delete such issues.

## Attribution
The logo icon made by [Gregor Cresnar](https://www.flaticon.com/authors/gregor-cresnar) from [www.flaticon.com](https://www.flaticon.com/) is licensed by [CC 3.0 BY](http://creativecommons.org/licenses/by/3.0/).
