[![Helm Package](https://img.shields.io/badge/helm-latest-blue.svg)](https://hub.helm.sh/charts/soluto/kamus) 
[![Slack](https://img.shields.io/badge/slack-kamus-orange.svg)](https://join.slack.com/t/k8s-kamus/shared_invite/enQtODA2MjI3MjAzMjA1LThlODkxNTg3ZGVmMjVkOTBhY2RmMmRjOWFiOGU2NzQ1ODU4ODNiMDJiZTE5ZTY4YmRiOTM3MjI0MDc0OGFkN2E)
[![Twitter](https://img.shields.io/twitter/follow/solutoeng.svg?label=Follow&style=popout)](https://twitter.com/intent/tweet?text=Checkout%20Kamus%20secret%20encryption%20for%20Kubernetes&url=https://github.com/Soluto/kamus&via=SolutoEng&hashtags=kubernetes,devops,devsecops) [![CircleCI](https://circleci.com/gh/Soluto/kamus.svg?style=svg)](https://circleci.com/gh/Soluto/kamus)

![logo](images/logo.png)  
# Kamus
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
Refer to the [installation guide](https://kamus.soluto.io/docs/user/install/) for more details.
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

Have a question? Something is not clear? Reach out to us on [Kamus Slack](https://join.slack.com/t/k8s-kamus/shared_invite/enQtODA2MjI3MjAzMjA1LThlODkxNTg3ZGVmMjVkOTBhY2RmMmRjOWFiOGU2NzQ1ODU4ODNiMDJiZTE5ZTY4YmRiOTM3MjI0MDc0OGFkN2E)!

## Architecture
Kamus has 3 components:
* Encrypt API
* Decrypt API
* Key Management System (KMS)

The encrypt and decrypt APIs handle encryption and decryption requests.
The KMS is a wrapper for various cryptographic solutions. Currently supported:
* AES - uses one key for all secrets
* AWS KMS, Azure KeyVault, Google Cloud KMS - creates one key per service account.

We look forward to add support for other cloud encryption backends.


Consult the [installation guide](https://kamus.soluto.io/docs/user/install) for more details on how to deploy Kamus using the relevant KMS.

### Utilities
Kamus is shipped with 2 utilities that make it easier to use:
* Kamus CLI - a small CLI that eases the interaction with the Encrypt API. Refer to the [docs](https://github.com/Soluto/kamus/blob/master/cli/README.md) for more details.
* Kamus init container - a init container that interacts with the Decrypt API. Refer to the [docs](https://github.com/Soluto/kamus/blob/master/init-container/README.md) for more details.
* CRD Controller - allowing to create native Kubernetes secrets using Kamus. Refer to the [docs](https://kamus.soluto.io/docs/user/crd/) for more details.

## Users
* [1 Giant Leap Solutions](https://1giantleap.nl/)
* [UK Hydrographic Office](https://www.ukho.gov.uk/)

Using Kamus? Open a PR and add your company name here!

## Security
We take security seriously at Soluto.
To learn more about the security aspects of Kamus refer to the Threat Modeling docs containing all the various threats and mitigations we discussed.
Before installing Kamus in production refer to the installation guide to learn the best practices of deploying Kamus securely.
In case you find a security issue or have something you would like to discuss refer to our [security.md](security.md) policy.

## Contributing
Found a bug? Have a missing feature? Please open an issue and let us know.
We would like to help you use Kamus!
Please notice: Do not report security issues on GitHub.
We will immediately delete such issues.
