---
title: "Architecture"
menu:
  main:
    parent: "Threat Modeling"
    identifier: "architecture"
    weight: 1
---

# Kamus Architecture
Kamus consist from 4 components:

* Encrypt API - handling encryption
* Decrypt API - handling decryption, should not be exposed externally
* KMS - Handling the encryption using various providers.
* Controller - Responsible for interavtion with Kubernetes API, currently only for CRUD operations on KamusSecrets objects.

## First flow - using the Init Container

High-level overview of encryption/decryption flow:
<img src="/docs/threatmodeling/images/diagram.png"/>

Let's take a deeper look of what's inside a pod:
<img src="/docs/threatmodeling/images/kamus-pod.png" height=600px style="display: block; margin-left: auto; margin-right: auto;"/>

We have multiple objects here:

* We have the pods, which run (at least) 2 containers, the application container and the init container.
* The config map, contains the encrypted secrets 
* An [emptyDir] (using memory medium) volume, shared between the containers.
* The init container, read the encrypted secrets, decrypt them and write a configuration file to the shared volume.
* The application container - this is where the user code is running. 
The application consume the configuration file with the decrypted secrets from the shared volume.
* A [service account], used by the init container to authenticate to Kamus decryptor.

## Second flow - using KamusSecret

High-level overview of encryption/decryption flow:
<img src="/docs/threatmodeling/images/diagram-crd.png"/>

Kubernetes Icons created by [Kubernetes Community]

[Kubernetes Community]: https://github.com/kubernetes/community
[emptyDir]: https://kubernetes.io/docs/concepts/storage/volumes/#emptydir
[service account]: https://kubernetes.io/docs/tasks/configure-pod-container/configure-service-account/
