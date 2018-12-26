# Kamus
An open source, git-ops, zero-trust secrets encryption and decryption solution for Kubernetes applications.
Kamus enable users to easily encrypt secrets than can be decrypted only by the application running on Kubernetes.
The encryption is done using strong encryptiong providers (currently supported: Azure KeyVault and AES).

## Getting Started

The simple way to run Kamus is by using the Helm chart:
```
helm upgrade --install incubator/kamus
```
Reffer to the installation guide for more details.
After installing Kamus, you can start using it to encrypt secrets.
First, you need to decide which pods need this secrets, associate these pods with the same service account.
Now, when you know the name of the service account, and the namespace it exist in, use Kamus CLI to encrypt the secret:
```
kamus-cli encrypt super-secret kamus-example-sa default --kamus-url <Kamus URL>
```
Pass the value returned by the CLI to your pod, and use Kamus to decrypt the value.
The simplest way to achieve that is by using the init container.
An alternative is to use Kamus decrypt API directly in the service code.
As part of this repo, you can find a full, working [example app](example/README).
You can deploy this app to any Kubernetes cluster that has Kamus installed, to understand how it works.

## Security
We take security seriously at Soluto. 
To learn more about the security aspects of Kamus, reffer to the Threat Modeling docs, containing all the various threats and mitigations we discussed.
Before installing Kamus in production, reffer the installation guide to learn the best practices of deploying Kamus securely.
In case you find a security issue, or have something you would like to discuss, reffer to our [Security.md](Security.md) policy.

## Contributing
Find a bug? Have a missing feature? Please open an issue and let us know. 
We would like to help you using Kamus!
Please notice: Do not report security issues on GitHub. 
We will delete imediatlly such issues.