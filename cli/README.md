## Kamus CLI    [![npm version](https://badge.fury.io/js/%40soluto-asurion%2Fkamus-cli.svg)](https://badge.fury.io/js/%40soluto-asurion%2Fkamus-cli)

This cli was created to provide an easy interface to interact with Kamus API.

It supports azure device flow authentication out of the box. 

---

#### Supported commands:

##### Encrypt
`node index.js encrypt <data> <serviceAccount> <namespace>`

---
#### How to enable azure active directory authentication 
You need working active directory [tenant](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-create-new-tenant) and designated [native app registration](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-v2-register-an-app), Then just set all the `auth` prefixed options.
Once the user will run the cli with the auth options, he will get a small code and and azure URL to login into.

---
##### CLI options:

| Option                | Required       |  Description                                     | Default Value |
| -------------------   | ------------   |  ----------------------------------------------- | ------------- |
| --auth-tenant         |   false        |  azure authentication tenant id                  |               |
| --auth-application    |   false        |  azure authentication application id             |               |
| --auth-resource       |   false        |  azure authentication resource id                |               |
| --cert-fingerprint    |   false        |  [certificate fingerprint](http://hassansin.github.io/certificate-pinning-in-nodejs) of encrypt api for validation       |               |
| --kamus-url           |   true         |  url of kamus encrypt    api                     |               |
| --allow-insecure-url  |   false        |  allow or block non https endpoints              | false         |
