# Kamus Example
A small example app, showing the power of Kamus. 
Before running this demo, make sure Kamus is up and running, and the CLI is installed.

## Running the demo
Before running the demo, make sure to install Kamus on the cluster. If Kamus is not installed, use the following command:
```
helm repo add soluto https://charts.soluto.io
helm upgrade --install soluto/kamus
```

Start by encrypting a secret using the CLI:
```
kamus-cli encrypt super-secret kamus-example-sa default --kamus-url <Kamus URL>
```
You might have to pass aditional arguments, based on your installation.

After encrypting the secret, open `deployment-kamus\configmap.yaml`.
Modify the value of `key` to the encrypted value returned from the CLI.

Now, run
```
kubectl apply -f deployment-kamus/
```
To deploy the example app. 
Check deployment status using
```
kubectl get pods
```
Notice the `kamus-example` pods. Wait for the pod to be in `Completed` state, and check the logs using
```
kubectl port-forward deployment/kamus-example 8080:80
```
You should see the following output
```
{"key":"super-secret"}
```
The example using an init container to decrypt the encrypted values. Checkout the documentation for additional details.

## Kubernetes Secrets
To complete the example, reffer to `deployment-secret`.
This example shows the alternative to Kamus - using Kubernetes native secrets.
Run the demo using
```
kubectl apply -f deployment-kamus/
```
Notice the `kamus-example` pods. Wait for the pod to be in `Completed` state, and check the logs using
```
kubectl logs -l app=kamus-example
```
You should see the following output
```
{"key":"super-secret"}
```
Editing the secrets:
* Open `secret.yaml`
* Decode the value under `config.json` using base64 decoder
* Edit the JSON
* Encode the JSON using base64 encoder, and put this value under `config.json`