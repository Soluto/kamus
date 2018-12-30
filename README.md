# Kamus ![logo](images/logo.png) 
An open source, git-ops, zero-trust secrets encryption and decryption solution for Kubernetes applications.
Kamus enable users to easily encrypt secrets than can be decrypted only by the application running on Kubernetes.
The encryption is done using strong encryption providers (currently supported: Azure KeyVault and AES).

## Getting Started

The simple way to run Kamus is by using the Helm chart:
```
helm upgrade --install incubator/kamus
```
Refer to the installation guide for more details.
After installing Kamus, you can start using it to encrypt secrets.
Kamus encrypt secrets for a specific application, represent by a [Kubernetes Service Account](https://kubernetes.io/docs/tasks/configure-pod-container/configure-service-account).
Creat a service account for your application, and mount it on the pods running your application. 
Now, when you know the name of the service account, and the namespace it exist in, use Kamus CLI to encrypt the secret:
```
npm install -g @soluto-asurion/kamus-cli
kamus-cli encrypt super-secret kamus-example-sa default --kamus-url <Kamus URL>
```
Pass the value returned by the CLI to your pod, and use Kamus Decrypt API to decrypt the value.
The simplest way to achieve that is by using the init container.
An alternative is to use Kamus decrypt API directly in the application code.
To make it clearer, take a look on a working [example app](example/README.md).
You can deploy this app to any Kubernetes cluster that has Kamus installed, to understand how it works.

## Architecture
Kamus has 3 components:
* Encrypt API
* Decrypt API
* Key Management System (KMS)

The encrypt and decrypt API handling encryption and decryption requests. 
The KMS is a wrapper for various cryptographic solutions. Currently supported:
* AES - using one key for all secrets
* Azure KeyVault - create one key per service account. 
We look forward to add support for other cloud solutions, like AWS KMS. 
If you're interested in such a failure, please let us know. 
We would like help with testing it out.
Consult the [installation guide](docs/install.md) for more details on how to deploy Kamus using the relevant KMS.
  
### Utilities
Kamus shipped with 2 utilities that make it easier to use:
* Kamus CLI - a small CLI that ease the interaction with the Encrypt API. Refer to the docs for more details.
* Kamus init container - a init container that interact with the Decrypt API. Refer to the docs for more details.

## Security
We take security seriously at Soluto. 
To learn more about the security aspects of Kamus, reffer to the Threat Modeling docs, containing all the various threats and mitigations we discussed.
Before installing Kamus in production, reffer the installation guide to learn the best practices of deploying Kamus securely.
In case you find a security issue, or have something you would like to discuss, reffer to our [security.md](security.md) policy.

## Contributing
Find a bug? Have a missing feature? Please open an issue and let us know. 
We would like to help you using Kamus!
Please notice: Do not report security issues on GitHub. 
We will immediately delete such issues.

## Attribution
The logo icon made by [Gregor Cresnar](https://www.flaticon.com/authors/gregor-cresnar) from [www.flaticon.com](https://www.flaticon.com/) is licensed by [CC 3.0 BY](http://creativecommons.org/licenses/by/3.0/).