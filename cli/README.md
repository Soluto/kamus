## Kamus CLI

This cli was created to give an easy interface to interact with Kamus API.

It supports azure device flow authentication out of the box. 

---

#### Supported commands:

##### Encrypt
`node index.js encrypt <data> <serviceAccount> <namespace>`

---
#### How to enable azure active directory authentication 
You need working active directory tenant and designated app registration, Then just set all the `auth` prefixed options.
Once the user will run the cli with the auth options, he will get a small code and and azure URL to login into.

---
##### CLI options:

| Option                | Required       |  Description                                     | Default Value |
| -------------------   | ------------   |  ----------------------------------------------- | ------------- |
| --auth-tenant         |   false        |  azure authentication tenant id                  |               |
| --auth-application    |   false        |  azure authentication application id             |               |
| --auth-resource       |   false        |  azure authentication resource id                |               |
| --cert-fingerprint    |   false        |  certificate fingerprint for requests validations|               |
| --kamus-url           |   true         |  url of kamus encryption api                     |               |
| --allow-insecure-url  |   false        |  allow or block non https endpoints              | false         |
