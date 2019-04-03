# Custom Resource Descriptor Support
Kamus supports also creating native Kubernetes secrets from an object called KamusSecret. 
KamusSecret is very similar to Secret, with one small difference - all the items in it are encrypted.
Using KamusSecret allows to use Kamus with applications that requires native Kubernetes secrets - for example, TLS secrets.

## Usage
KamusSecret works very similary to regular secret encryption flow with Kamus.
To encrypt the data, start by deciding to which namespace and which service account you're encrypting it.
The service account does not have to exist or used by the pod consuming the secret.
It just used for expressing who can consume this encrypted secret.
Use the [CLI](../cli/README.md) to encrypt the data:
```
kamus-cli encrypt --secret super-secret --service-account kamus-example-sa --namespace default --kamus-url <Kamus URL>
```
Now that you have the data encrypted, create a KamusSecret object, using the following manifest:
```
apiVersion: "soluto.com/v1alpha1"
kind: KamusSecret
metadata:
  name: my-tls-secret     //This will be the name of the secret
  namespace: default      //The secret and KamusSecret live in this namespace
type: TlsSecret           //The type of the secret that will be created
data:                     //Put here all the encrypted data, that will be stored (decrypted) on the secret data
  key: J9NYLzTC/O44DvlCEZ+LfQ==:Cc9O5zQzFOyxwTD5ZHseqg==
serviceAccount: some-sa   //The service account used for encrypting the data
```
And finally, create the KamusSecret using:
```
kubectl apply -f kamussecret.yaml
```
And after a few seconds you'll see the new secret created:
```
$ kubectl get secrets
NAME                                       TYPE                                  DATA   AGE
default-token-m6whl                        kubernetes.io/service-account-token   3      1d
my-tls-secret                              TlsSecret                             1      5s
```

## Known limitation
This is the alpha release of this feature, so not all functionality is supported. 
The current known issues:
* There is no support for updating KamusSecrets object - only add/delete
* There is no validation - so if you forgot to add mandatory keys to the KamusSecret objects, it will not be created properly.
