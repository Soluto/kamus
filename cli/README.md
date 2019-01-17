[![npm version](https://badge.fury.io/js/%40soluto-asurion%2Fkamus-cli.svg)](https://badge.fury.io/js/%40soluto-asurion%2Fkamus-cli)
[![Known Vulnerabilities](https://snyk.io/test/github/soluto/kamus/badge.svg?targetFile=cli/package.json)](https://snyk.io/test/github/soluto/kamus) [![docker hub](https://images.microbadger.com/badges/image/soluto/kamus-cli.svg)](https://hub.docker.com/r/soluto/kamus-cli "Get your own image badge on microbadger.com")

## Kamus CLI

This cli was created to provide an easy interface to interact with Kamus API.

It supports azure device flow authentication out of the box.

To install, use the following NPM command:
```
npm install -g @soluto-asurion/kamus-cli
```
Alternatively, you can use docker to run the CLI (for example, to run it inside the cluster when the encryptor is deployed without ingress):
```
docker run -it --rm soluto/kamus-cli encrypt <arguments>
```
Or, using kubectl:
```
kubectl run -it --rm --restart=Never kamus-cli --image=soluto/kamus-cli -- encrypt <arguments>
```
---

#### Supported commands:

##### Encrypt
`kamus-cli encrypt --secret <data> --service-account <serviceAccount> --namespace <namespace> --kamus-url <kamus-url> `

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
