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
### Decryption

* [Impersonating pod to decrypt it's secrets](/docs/threatmodeling/threats/decryption/pod_impersonation)
* [Sniff requests and responses to Kamus](/docs/threatmodeling/threats/decryption/sniffing_tampering)
* [Using KamusSecret to decrypt secrets](/docs/threatmodeling/threats/decryption/leveraging_crd)

### Encryption
* [Kamus server disrupting](/docs/threatmodeling/threats/encryption/denial_of_service)
* [Exposing names of namespaces and service accounts](/docs/threatmodeling/threats/encryption/namespace_enumeration)
* [Sniffing user's traffic](/docs/threatmodeling/threats/encryption/sniffing_user_traffic)

### KMS
* [Accessing KMS with leaked credentials](/docs/threatmodeling/threats/kms/leaked_credentials)
* [Breaking encryption key](/docs/threatmodeling/threats/kms/quantom_computing)

## Controls
### Decryption

* [Deny default service account](/docs/threatmodeling/controls/decryption/deny_default_sa)
* [Deny Kuberentes secrets view permissions](/docs/threatmodeling/controls/decryption/deny_secret_view)
* [Use TLS when accessing Kuberentes API](/docs/threatmodeling/controls/decryption/k8s_api_tls)
* [Serve Kamus API over TLS](/docs/threatmodeling/controls/decryption/kamus_in_cluster_tls)
* [Use a policy to control pod's service account](/docs/threatmodeling/controls/decryption/opa_pods_secrets)

### Encryption

* [Block anonymous internet access to Kamus](/docs/threatmodeling/controls/encryption/block_internet_access)
* [Certificate pinning](/docs/threatmodeling/controls/encryption/certificate_pinning)
* [Client-side encryption](/docs/threatmodeling/controls/encryption/client_side_encryption)
* [Deny request for default SA](/docs/threatmodeling/controls/encryption/deny_default_sa)
* [Throttling requests by source IP](/docs/threatmodeling/controls/encryption/ip_throttling)

### Key Management System
* [Enable firewall protection for KMS](/docs/threatmodeling/controls/kms/firewall_protection)
* [Credentials Hardening](/docs/threatmodeling/controls/kms/hardening_credentials)
* [Key names obfuscation](/docs/threatmodeling/controls/kms/obfuscate_key_names)
* [Use KMS with HSM support](/docs/threatmodeling/controls/kms/use_hsm)