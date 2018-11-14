# Kamus
An open source, git-ops, zero-trust secrets encryption and decryption solution for Kubernetes applications.
Kamus enable users to easily encrypt secrets than can be decrypted only by the application running on Kubernetes.
The encryption is done using strong encryptiong providers (currently supported: Azure KeyVault and AES).

## Getting Started

The simple way to run Kamus is by using the Helm chart.
After installing Kamus, you can start using it to encrypt secrets.
Before encrypting a secret, you will need a Kubernetes service account.
The service account is used to authenticate your application in production. 
Only applications tha tare running with this service account, will be able to decrypt the secret.
After create the service account, use the following HTTP request to encrypt your secret:
```
POST /api/v1/encrypt HTTP/1.1
Host: <Kamus Server URL>
Content-Type: application/json
Cache-Control: no-cache
{
	"data": "super secret",
	"service-account": "<name of service account>",
	"namespace": "<name of your namespace>"
}
```

Than, use the decryptor init container to decrypt the secret in production.

## Security