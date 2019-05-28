# Running the init container locally

1. Make sure you have a working Kubernetes cluster (run `kubectl get pods` to check).
2. Start by running the decryptor API - run the following in the decryptor folder (`src/decrypt-api`):
```
dotnet run
```
3. Run the following command to get a valid service account token:
```
kubectl get secret $(kubectl get sa default -o jsonpath='{.secrets[0].name}') -o jsonpath='{.data.token}' | base64 -D > token.txt
```
4. Set the following env var for the init container to work correctly:
```
export set TOKEN_FILE_PATH=$(pwd)/token.txt
export set KAMUS_URL=http://localhost:5000
```
5. Now you can run the init container:
```
node index.js -e encrypted -d decrypted -n output.json 
```

The first argument (`-e`) point to the folder with the encrypted files, the second (`-d`) point to the folder that will contain the decrypted items and (`-n`) is the decrypted file name.

And you should see a file name `output.json` created under `decrypted` folder with the decrypted content.
Now you can run the init continer locally, debug it and add features. 

Having more questions? Something unclear? Reach out to us via [Slack](https://join.slack.com/t/k8s-kamus/shared_invite/enQtNTQwMjc2MzIxMTM3LTgyYTcwMTUxZjJhN2JiMTljMjNmOTBmYjEyNWNmZTRiNjVhNTUyYjMwZDQ0YWQ3Y2FmMTBlODA5MzFlYjYyNWE) or [file an issue](https://github.com/Soluto/kamus/issues/new)

## Tests
The tests are using [WireMock](http://wiremock.org/) to mock the decryptor api (you can find ore about it [here](https://www.omerlh.info/2019/02/06/wiremock-for-fun-and-mocking/)).
Running the tests is simple:
1. Build the init container docker image:
```
yarn run build
``` 
2. Set the environment variable so the tests will use the local image:
```
export set INIT_CONTAINER_IMAGE=init-container
```
3. Run the tests
```
yarn run test
```

Repeat steps 1 & 3 each time you change the code.

The tests simply take various parameters and compare the generated file from the init container with the expected files. 