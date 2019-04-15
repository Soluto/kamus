An open source, GitOps, zero-trust secrets encryption and decryption solution for Kubernetes applications.
Kamus enable users to easily encrypt secrets than can be decrypted only by the application running on Kubernetes.
The encryption is done using strong encryption providers (currently supported: Azure KeyVault, Google Cloud KMS, Amazon Web Services KMS and AES).
To learn more about Kamus, check out the [blog post](https://blog.solutotlv.com/can-kubernetes-keep-a-secret?utm_source=github) and [slides](https://www.slideshare.net/SolutoTLV/can-kubernetes-keep-a-secret).

## Utilities
Kamus is shipped with 3 utilities that make it easier to use:

* Kamus CLI - a small CLI that eases the interaction with the Encrypt API. Refer to the docs for more details.
* Kamus init container - a init container that interacts with the Decrypt API. Refer to the docs for more details.
* CRD Controller - allowing to create native Kubernetes secrets using Kamus. Refer to the [docs](docs/user/crd) for more details.

## Security
We take security seriously at Soluto.
To learn more about the security aspects of Kamus refer to the Threat Modeling docs containing all the various threats and mitigations we discussed.
Before installing Kamus in production refer to the installation guide to learn the best practices of deploying Kamus securely.
In case you find a security issue or have something you would like to discuss refer to our [responsible disclousre](docs/threatmodeling/security) policy.

## Contributing
Found a bug? Have a missing feature? Please open an issue and let us know.
We would like to help you use Kamus!
Please notice: Do not report security issues on GitHub.
We will immediately delete such issues.

## Attribution
Kamus docs are based on [kind] docs. All the content was replaced, but the underline framework is the same. Thank you Kind team for building your website!

[kind]: https://kind.sigs.k8s.io