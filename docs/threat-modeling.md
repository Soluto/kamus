# Kamus Threat Modeling

![alt text](./diagram.png)

This doc contains the threat modeling conducted for Kamus. It listed the threats that were discussed, and the mitigation available agains these threats.

## Encryption
* DoS - An attacker can use the encryption endpoint to DoS our KMS. This is why there is another step of validation - using Kubernetes API to make sure the namespace exist (Not good enough mitigation, we now DoS Kubernetes + hacker can use `default/default` and than DoS). Another mitigation - adding rate limit, consider adding some sort of authentication.
* Oracle attack - using authentication endpoint to discover the encryption key. Mitigation - create key per namespace/service account. 
* Information disclouser - leaking information in error message. Mitigation - code review, Zap scanning
* Weak encryption algorithm - use KMS built-in algorithms.

## Decryption
* A privileged attacker can launch a pod with a service account used by another pod. By doing so, the attacker can use this token to impersonate the pod and decrypt it's secrets.
* An attacker can perform MitM attack and still tokens/decrypted data as the in-cluster communication is not encrypted.
* Kubernetes token does not expire, so once a token is compromised, an attacker has infinite access. Compromising a secret is relatively simple - any user with permissions to get secret, can get service account tokens.