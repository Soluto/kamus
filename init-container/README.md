[![CircleCI](https://circleci.com/gh/Soluto/kamus.svg?style=svg)](https://circleci.com/gh/Soluto/kamus) [![Known Vulnerabilities](https://snyk.io/test/github/soluto/kamus/badge.svg?targetFile=init-container/package.json)](https://snyk.io/test/github/soluto/kamus) [![Dockerhub](https://images.microbadger.com/badges/image/soluto/kamus-init-container.svg)](https://microbadger.com/images/soluto/kamus-init-container "Get your own image badge on microbadger.com")
# Kamus Init Container
A [init container](https://kubernetes.io/docs/concepts/workloads/pods/init-containers/) that decrypt secrets using Kamus decryptor API and write them to a file.

## Getting Started
The simplest way to use the init container is by creating a config map to store the encrypted values:
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: encrypted-secrets-cm
data:
  key: 4AD7lM6lc4dGvE3oF+5w8g==:WrcckiNNOAlMhuWHaM0kTw==
```
Use the CLI or direct API calls to encrypt the values.
The init container has 2 mounted volumes:
* Encrypted items: mounted from the config map, contains all the encrypted values
* Decrypted items: the init container will write all the decrypted items to this volume. The vulme medium is memory for increased security.

Take a look on the [deployment](example/deployment-kamus/deployment.yaml) of the example app to see how it's all connected together. You'll notice that app container and the init container, and you can see the mount settings. Don't forget to mount the decrypted item into the container running the app.

## Usage
The init container accept the following environmenmt variables:

| Option                | Required       |  Description                                     | Default Value |
| -------------------   | ------------   |  ----------------------------------------------- | ------------- |
| -V/--version          |   false        |   output the version number                |               |
| `-e/--encrypted-folder <path>`          |   true        |   Encrypted files folder path (the volume mounted with the config map)               |               |
| `-d/--decrypted-path <path>`          |   false        |   Decrypted file/s folder path mounted. Pass this argument to create one decrypted file per encrypted secret              |               |
| `-n/--decrypted-file-name <name>`          |   false        |   Decrypted file name. Pass this argument to create one configuration file with the encrypted secrets.             |               |
| `-f/--output-format <format>`          |   false        |  The format of the output file, default to JSON. Supported types: json, cfg, files           |         JSON      |