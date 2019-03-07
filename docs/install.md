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

## Supported KMS Providers

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
helm upgrade --install kamus soluto/kamus --set keyManager.AES.key=$key
```

### Azure KeyVault KMS
Using [Azure KeyVault](https://azure.microsoft.com/en-us/services/key-vault/) as the key managment solution is the secure solution when running a cluster on Azure.
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
keyManagment:
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
* Start by creating a service account: `gcloud iam service-accounts create kamus`
* Assing the service account the required permissions:
```
gcloud projects add-iam-policy-binding <project id> --member "serviceAccount:kamus@<project id>.iam.gserviceaccount.com" --role "roles/cloudkms.cryptoKeyEncrypterDecrypter"
gcloud projects add-iam-policy-binding <project id> --member "serviceAccount:kamus@<project id>.iam.gserviceaccount.com" --role "roles/cloudkms.admin"
```
Please note: There is no exact role with all the required permissions for Kamus. It is recommended to create a custom role with the following permissions: `cloudkms.cryptoKeys.get`, `cloudkms.cryptoKeys.create`, `cloudkms.cryptoKeyVersions.useToEncrypt`, `cloudkms.cryptoKeyVersions.useToDecrypt`.
* Generate keys for the service:
```
gcloud iam service-accounts keys create credentials.json --iam-account kamus@[PROJECT_ID].iam.gserviceaccount.com
```

Now add the following to your `values.yaml` file:
```yaml
keyManagement:
  provider: GoogleKms
  googleKms:
    location: <location>
    keyRing: <key ring name>
    protectionLevel: HSM
```
And use the following command to deploy kamus:
```
 helm upgrade --install kamus soluto/kamus -f values.yaml --set-string keyManagement.googleKms.credentials="$(cat credentials.json | base64)"
```

### AWS KMS
Using [AWS KMS](https://docs.aws.amazon.com/kms/latest/developerguide/overview.html) as the key managment solution is the secure solution when running a cluster on AWS Cloud.
There are 2 options to authentication with the KMS:
1. Kamus by default will try to use the regular AWS SDK discovery mechinisem, if your cluster in AWS you need to map IAM role to kamus POD by using one of the community tools, for example [kiam](https://github.com/uswitch/kiam).
2. Provide user access key and secret with KMS access.

Typical values.yaml for AWS :
```yaml
keyManagement:
  provider: AwsKms
```
If you want to pass user access key and secret to Kamus deploy use the following values.yaml command:
```yaml
keyManagement:
  provider: AwsKms
  awsKms:
    region: <>
    key: <>
    secret: <>
```
And now deploy Kamus using the following helm command:
```
helm upgrade --install kamus soluto/kamus -f <path/to/values.yaml>
```

