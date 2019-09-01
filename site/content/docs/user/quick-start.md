---
title: "Quick Start"
menu:
  main:
    parent: "user"
    identifier: "quick-start"
    weight: 1
---

# Quick Start

The simple way to run Kamus is by using the Helm chart:
```
helm repo add soluto https://charts.soluto.io
helm upgrade --install kamus soluto/kamus
```
Refer to the [installation guide](/docs/user/install) to learn about production grade deployment.
After installing Kamus, you can start using it to encrypt secrets.
Kamus encrypt secrets for a specific application, represent by a [Kubernetes Service Account](https://kubernetes.io/docs/tasks/configure-pod-container/configure-service-account).
Create a service account for your application, and mount it on the pods running your application.
Now, when you know the name of the service account, and the namespace it exists in, install Kamus CLI:
```
npm install -g @soluto-asurion/kamus-cli
```
Use Kamus CLI to encrypt the secret:
```
kamus-cli encrypt \\
    --secret super-secret \\
    --service-account kamus-example-sa \\
    --namespace default \\
    --kamus-url <Kamus URL> \\
```

When Kamus URL is a url pointing to the encryptor pod of Kamus, exposed either via port forward (e.g. `kubectl port-forward svc/kamus-encryptor 9999:9999`) or via ingress.
In case of non-https Kamus URL (e.g. `http://localhost:<port>`), you'll have to add the `--allow-insecure-url` flag to enable http protocol.

Pass the value returned by the CLI to your pod, and use Kamus Decrypt API to decrypt the value.
The simplest way to achieve that is by using the init container.
An alternative is to use Kamus decrypt API directly in the application code.
To make it clearer, take a look on a working [example app](example/README.md).
You can deploy this app to any Kubernetes cluster that has Kamus installed, to understand how it works.

Have a question? Something is not clear? Reach out to us on [Kamus Slack](https://join.slack.com/t/k8s-kamus/shared_invite/enQtNTQwMjc2MzIxMTM3LTgyYTcwMTUxZjJhN2JiMTljMjNmOTBmYjEyNWNmZTRiNjVhNTUyYjMwZDQ0YWQ3Y2FmMTBlODA5MzFlYjYyNWE)!
