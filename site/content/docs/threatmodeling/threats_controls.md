---
title: "Threats and Controls"
menu:
  main:
    parent: "Threat Modeling"
    identifier: "threat_controls"
    weight: 1
---

# Threats and Controls
This page document the output of Kamus threat model, by listing all the different threats and mitigations that we discussed.

## Threats
### Controls
#### Decryption

* [Deny default service account](/docs/threatmodeling/controls/decryption/deny_default_sa)
* [Deny Kuberentes secrets view permissions](/docs/threatmodeling/controls/decryption/deny_secret_view)
* [Use TLS when accessing Kuberentes API](/docs/threatmodeling/controls/decryption/k8s_api_tls)