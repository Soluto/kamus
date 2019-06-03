An open source, GitOps, zero-trust secrets encryption and decryption solution for Kubernetes applications.
Kamus enable users to easily encrypt secrets than can be decrypted only by the application running on Kubernetes.
The encryption is done using strong encryption providers (currently supported: Azure KeyVault, Google Cloud KMS, Amazon Web Services KMS and AES).
To learn more about Kamus, check out the [blog post](https://blog.solutotlv.com/can-kubernetes-keep-a-secret?utm_source=github) and a [talk](https://www.youtube.com/watch?v=FoM3u8G99pc&&index=14&t=0s) from AppSec California 2019.

## Overview
Kamus offers two different mechanism for consuming encrypted secret:

* Store them on a config map and use the [init container] to decrypt them. 
The init container will produce a configuration file that can be consumed by the application with the decrypted secrets.
* Create a [KamusSecret][crd] that contains the encrypted data. 
Kamus will decrypt the items in the KamusSecret and create a regular Kubernetes Secret object with the decypted items. 
The application can consume this Secret like any other Secret (refer to the [docs][secrets] for more details). 

Please note: the second flow (KamusSecrets) is intended to be used by application that cannot use the regular secrets (mainly 3rd party applications) or for specific Secrets types (like TLS secrets or docker registry credentials). 
Kubernetes Secrets has some limitations, especially when consuming them as volume mounts. 
We recommend the usage of the first flow for most use cases.

## Utilities
Kamus is shipped with 3 utilities that make it easier to use:

* Kamus CLI - a small CLI that eases the interaction with the Encrypt API. Refer to the docs for more details.
* [Kamus init container][init container] - a init container that interacts with the Decrypt API. Refer to the docs for more details.
* CRD Controller - allowing to create native Kubernetes secrets using Kamus. Refer to the [docs][crd] for more details.

## Security
We take security seriously at Soluto.
To learn more about the security aspects of Kamus refer to the Threat Modeling docs containing all the various threats and mitigations we discussed.
Before installing Kamus in production refer to the installation guide to learn the best practices of deploying Kamus securely.
In case you find a security issue or have something you would like to discuss refer to our [responsible disclousre](docs/threatmodeling/security) policy.

## Alternatives to Kamus
Kamus designed to support GitOps flow. 
There are other projects you can use to encrypt secrets for Kubernetes, for example:

* [Sealed Secrets](https://github.com/bitnami-labs/sealed-secrets)
* [Helm Secrets](https://github.com/futuresimple/helm-secrets)

There are also good solutions that let you have a centralized location for storing secrets, for example:

* [Hashicorp Vault](https://www.vaultproject.io/)
* [Lyft Confidant](https://github.com/lyft/confidant)
* [AWS SSM Parameter Store](https://docs.aws.amazon.com/systems-manager/latest/userguide/systems-manager-parameter-store.html)
* [Azure KeyVault](https://azure.microsoft.com/en-us/services/key-vault/)

Kamus works a bit different from those projects. This is discussed as part of this [talk](https://www.youtube.com/watch?v=FoM3u8G99pc&&index=14&t=0s).
## Contributing
Found a bug? Have a missing feature? Please open an issue and let us know.
We would like to help you use Kamus!
Please notice: Do not report security issues on GitHub.
We will immediately delete such issues.

## Attribution
Kamus docs are based on [kind] docs. All the content was replaced, but the underline framework is the same. Thank you Kind team for building your website!

[kind]: https://kind.sigs.k8s.io
[init container]: https://github.com/Soluto/kamus/tree/master/init-container
[crd]: docs/user/crd
[secrets]: https://kubernetes.io/docs/concepts/configuration/secret/