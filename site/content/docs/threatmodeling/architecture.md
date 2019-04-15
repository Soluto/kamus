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

This is the flow of encryption/decryption:
<img src="/docs/threatmodeling/images/diagram.png"/>

This is the flow of the Controller:
<img src="/docs/threatmodeling/images/diagram-crd.png"/>
