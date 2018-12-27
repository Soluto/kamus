# Installing Kamus
Kamus has an official helm chart, using it is the simplest way to install Kamus:
```
helm upgrade --install incubator/kamus
```
Careful - using this command will deploy Kamus with the default encryption keys.
Meaning, anyone could decrypt the data that Kamus encrypt.
This is fine for testing and playing with Kamus, but not for production installations.
For production usage, please configuration one of the supported KMS.

## AES KMS
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
helm upgrade --install kamus incubator/kamus --set keyManager.AES.key=$key
```

## Azure KeyVault KMS
Using [Azure KeyVault](https://azure.microsoft.com/en-us/services/key-vault/) as the key managment solution is a more secure solution.
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
helm upgrade --install kamus incubator/kamus -f <path/to/values.yaml>
```
