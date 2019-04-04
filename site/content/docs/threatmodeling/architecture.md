---
title: "Architecture"
menu:
  main:
    parent: "Threat Modeling"
    identifier: "architecture"
    weight: 1
---

# Kamus Architecture
Kamus consist from 3 components:

* Encrypt API - handling encryption
* Decrypt API - handling decryption, should not be exposed externally
* KMS - Handling the encryption using various providers.

This is the flow of encryption/decryption:
<img src="/docs/threatmodeling/images/diagram.png"/>
