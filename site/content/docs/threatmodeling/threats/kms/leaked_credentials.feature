# Id: KAMUS-T-K-1
# Status: Confirmed
# Components:
#   - Secret decryption
#   - Secret encryption
# STRIDE:
#   - Spoofing
#   - Information Disclosure 
# References:
#   - https://kubernetes.io/docs/tasks/configure-pod-container/configure-service-account/
#   - https://kubernetes.io/docs/concepts/configuration/secret/#service-accounts-automatically-create-and-attach-secrets-with-api-credentials
#   - https://kubernetes.io/docs/reference/generated/kubernetes-api/v1.12/#tokenreview-v1-authentication-k8s-io

Feature: Accessing KMS with leaked credentials
  In order to decrypt pod's secrets
  As an attacker
  I can access KMS directly

  Scenario Outline: Used leaked credentials
    Given KMS compromised
    When the attacker call KMS with these credentials
    Then the attacker can decrypt any pod secrets, until the credentials expired

    Examples: Data types
      | data-type         |
      | password          |
      | API key           |
      | X.509 private key |
      | SSH private key   |
