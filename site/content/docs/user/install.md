---
title: "Installing Kamus"
menu:
  main:
    parent: "user"
    identifier: "user-install"
    weight: 2
---

# Installing Kamus
Kamus has an official helm chart, using it is the simplest way to install Kamus:
```
helm repo add soluto https://charts.soluto.io
helm upgrade --install kamus soluto/kamus
```
Careful - using this command will deploy Kamus with the default encryption keys.
Meaning, anyone could decrypt the data that Kamus encrypt.
This is fine for testing and playing with Kamus, but not for production installations.
For production usage, please configuration one of the supported Key Management Solutions (KMS).

## Contents

* [AES KMS](#aes-kms)
* [Azure Keyvault KMS](#azure-keyvault-kms)
* [Google Cloud KMS](#google-cloud-kms)
* [AWS KMS](#aws-kms)
* [Installing without helm](#installing-without-helm)

### AES KMS
AES KMS is the simplest (but less secure) solution. 
Kamus will use one strong AES key to encrypt all the data. 
Currently, rolling this key is not supported.
To deploy Kamus using AES Key:
* Generate a strong AES key:
```
key=$(openssl rand -base64 32 | tr -d '\n')
```
* Pass the value when deploying kamus, either using `values.yaml` or directly in the helm command:
```
helm upgrade --install kamus soluto/kamus --set keyManagement.AES.key=$key
```

### Azure KeyVault KMS
Using [Azure KeyVault](https://azure.microsoft.com/en-us/services/key-vault/) as the key management solution is the most secure solution when running a cluster on Azure.
Azure documentation is far from perfect, so I'm going to reffer to a lot of different guides because there is no one guide documenting the required process.

Start by creating a KeyVault instance. 
It is recommend to create a KeyVault with HSM backend for additional security. 
Follow this [guide](https://docs.microsoft.com/en-us/azure/key-vault/key-vault-manage-with-cli2#working-with-hardware-security-modules-hsms) for details on how to create a KeyVault using the CLI. It is recommend to protect the KeyVault with firewall, see this [guide](https://docs.microsoft.com/en-us/azure/key-vault/key-vault-network-security) for additional details.

After creating a KeyVault instance, Kamus need permissions to access it.
You grant Kamus permissions by creating an Azure Active Directory application for Kamus, and granting permissions for this application to access the KeyVault created in the previous step. 
Creating the required app is covered in 2 parts of the same guide. The [first part](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal#create-an-azure-active-directory-application) will guide you through the process of creating the app. The [second part](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal#get-application-id-and-authentication-key) will guide you through the process of creating the client id and client secret, that are used by Kamus for authentication. Try to create the client secret for short period, for example 6 months, and rotate it frequently.

Now you should have 3 objects: KeyVault, client id and client secret. The last part is to grant the application the required permissions on the KeyVault. First we need to get the object id of the application:
```
objectId=$(az ad app show --id <> --output json | jq '.objectId' -r)
```
Now use the following command to grant access:
```
az keyvault set-policy --name <> --object-id $objectId --key-permissions get list create encrypt decrypt
```

Now it's time to deploy Kamus! Use the following settings in your `values.yaml` file:
```
keyManagement:
  provider: AzureKeyVault
  azureKeyVault:
    clientId: <>
    clientSecret: <>
    keyVaultName: <>
    keyType: RSA-HSM //change to RSA if you choosed not to use premium SKU
    keySize: 2048
    maximumDataLength: 214
```
And now deploy Kamus using the following helm command:
```
helm upgrade --install kamus soluto/kamus -f <path/to/values.yaml>
```

### Google Cloud KMS
Using [Google Cloud KMS](https://cloud.google.com/kms/) as the key managment solution is the secure solution when running a cluster on Google Cloud.
For a more secure installation, it is recommended to use a keys that are HSM-protected (see [Cloud HSM](https://cloud.google.com/kms/docs/hsm) documentation). Before using Google Cloud KMS, make sure the api is [enabled](https://console.cloud.google.com/flows/enableapi?apiid=cloudkms.googleapis.com&redirect=https://console.cloud.google.com&_ga=2.90411866.-1791338329.1542008700).

To interact with Google Cloud KMS, Kamus needs an existing key ring and a service account.
To create a key ring, run the following command:
```
gcloud kms keyrings create <key ring name> --location <location>
```
If you plan to use HSM protection, choose a region that is supported - you can find the full list [here](https://cloud.google.com/kms/docs/locations#hsm_regions).

To create a service account, run the following commands:
* Start by creating a service account: `gcloud iam service-accounts create kamus-sa`
* Assing the service account the required permissions:
```
gcloud projects add-iam-policy-binding <project id> --member "serviceAccount:kamus-sa@<project id>.iam.gserviceaccount.com" --role "roles/cloudkms.cryptoKeyEncrypterDecrypter"
gcloud projects add-iam-policy-binding <project id> --member "serviceAccount:kamus-sa@<project id>.iam.gserviceaccount.com" --role "roles/cloudkms.admin"
```
Please note: There is no exact role with all the required permissions for Kamus. It is recommended to create a custom role with the following permissions: `cloudkms.cryptoKeys.get`, `cloudkms.cryptoKeys.create`, `cloudkms.cryptoKeyVersions.useToEncrypt`, `cloudkms.cryptoKeyVersions.useToDecrypt`.
* Generate keys for the service:
```
gcloud iam service-accounts keys create credentials.json --iam-account kamus-sa@[PROJECT_ID].iam.gserviceaccount.com
```

Now add the following to your `values.yaml` file:
```yaml
keyManagement:
  provider: GoogleKms
  googleKms:
    projectId: <project id>
    location: <location>
    keyRing: <key ring name>
    protectionLevel: HSM
    rotationPeriod: <optional, the period for automatic key rotation>
```
And use the following command to deploy kamus:
```
 helm upgrade --install kamus soluto/kamus -f values.yaml --set-string keyManagement.googleKms.credentials="$(base64 credentials.json | tr -d \\n)"
```

Automatic credentials rotation is supported by Google Cloud KMS (see the docs [here][gcp kms key rotation]). To enable it, just set `keyManagement.googleKms.rotationPeriod` to the desired period. The value is using [C# Time Span Format][timespan], which is simply the number of days you want. According to the docs, rotating the keys does not affect existing encrypted values - while the old version exist, decryption should work as expected.

### AWS KMS
Using [AWS KMS](https://docs.aws.amazon.com/kms/latest/developerguide/overview.html) as the key management solution is the most secure solution when running a cluster on AWS Cloud.
The required permissions for the IAM role/user for Kamus to work properly are KMS permissions for Encrypt, Decrypt, and GenerateDataKey.

There are 2 options to authentication with the KMS:

1. Kamus by default will try to use the regular AWS SDK discovery mechanism, if your cluster in AWS you need to map IAM role to kamus POD by using one of the community tools, for example [kiam](https://github.com/uswitch/kiam).
2. Provide user access key and secret with KMS access.

Typical values.yaml for AWS :
```yaml
keyManagement:
  provider: AwsKms
  region: <the region to use for AWS KMS, if not specified by the instance metadata>
```
If you want to pass user access key and secret to Kamus deploy use the following values.yaml command:
```yaml
keyManagement:
  provider: AwsKms
  awsKms:
    region: <>
    key: <>
    secret: <>
    enableAutomaticKeyRotation: <optional, set to true if you want>
```
You can also provide `cmkPrefix` values to give the custerom master keys that Kamus creates better visibility, if not specific the keys alias will be called `kamus-<GUID>`. 
And now deploy Kamus using the following helm command:
```
helm upgrade --install kamus soluto/kamus -f <path/to/values.yaml>
```

Automatic key rotation is support by AWS KMS (see the documentation [here][aws kms key rotation]). When enabled, AWS will rotate the credentials once every year. To enable it, just set `keyManagement.awsKms.enableAutomaticKeyRotation` to true. According to the docs, rotating the keys does not affect existing encrypted values - while the old version exist, decryption should work as expected.

### Installing Without Helm
While Helm is the easiest way to install Kamus, it is not mandatory.
You can use [helm template] to generate the raw Kubernetes manifest files and than install Kamus using kubectl:

* Download and install [helm] and [helm template]. 
You can initialize Helm using `helm init -c`, which allows you to use Helm locally, without a connected cluster. Make sure to add soluto helm repository, as specified above.
* Create `values.yaml` according to your needs. Follow the instructions above and choose the method that fits your environment.
* Run the following command to generate the manifest file:
```
helm fetch soluto/kamus --untar && helm template kamus -f values.yaml > manifest.yaml
```
* Now use `kubectl` to install Kamus:
```
kubectl apply -f manifest.yaml
```

And you're done! Kamus is now installed on your cluster. 

[helm template]: https://github.com/technosophos/helm-template
[helm]: https://helm.sh/
[gcp kms key rotation]: https://cloud.google.com/kms/docs/key-rotation
[aws kms key rotation]: https://docs.aws.amazon.com/kms/latest/developerguide/rotate-keys.html
[timespan]: https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-timespan-format-strings
