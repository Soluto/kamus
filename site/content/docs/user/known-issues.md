---
title: "Known Issues"
menu:
  main:
    parent: "user"
    identifier: "known-issues"
    weight: 4
---
# Known Issues

Having problems with Kamus? This guide is covers some known problems and solutions / workarounds.

It may additionally be helpful to:

- check our [issue tracker]
- [file an issue][file an issue] (if there isn't one already)
- reach out and ask for help on the [kamus slack][kamus slack] (use the [slack invite] link)

## Contents
* [Encryption failure](#encryption-failure)

## Encryption failure
You might experience an issue when trying to encrypt with the CLI, similar to the following:
```
[error kamus-cli]: Error while trying to encrypt with kamus: Encrypt request failed due to unexpected error. Status code: <>
```

When this happens, try to check the following:

* Status code 400? Might be because you tried to encrypt a secret for the default service account in a namespace? It's currently not supported by Kamus (see [deny default sa] control for more details). There is open issue ([#130]) to improve the error message.
* Status code 500? Check Kamus error logs using (there is an open issue [#120] to improve the error message):

```
kubectl logs -l "app=kamus,component=encryptor" --since 5m
```

[issue tracker]: https://github.com/Soluto/Kamus/issues
[file an issue]: https://github.com/Soluto/Kamus/issues/new
[kamus slack]: http://k8s-kamus.slack.io/
[slack invite]: https://join.slack.com/t/k8s-kamus/shared_invite/enQtODA2MjI3MjAzMjA1LThlODkxNTg3ZGVmMjVkOTBhY2RmMmRjOWFiOGU2NzQ1ODU4ODNiMDJiZTE5ZTY4YmRiOTM3MjI0MDc0OGFkN2E
[deny default sa]: /docs/threatmodeling/controls/decryption/deny_default_sa
[#130]: https://github.com/Soluto/kamus/issues/130
[#120]: https://github.com/Soluto/kamus/issues/122
