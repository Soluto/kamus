# Id: KAMUS-T-D-1
# Status: Confirmed
# Components:
#   - Secret decryption
# STRIDE:
#   - Spoofing
#   - Information Disclosure 
# References:
#   - https://kubernetes.io/docs/tasks/configure-pod-container/configure-service-account/
#   - https://kubernetes.io/docs/concepts/configuration/secret/#service-accounts-automatically-create-and-attach-secrets-with-api-credentials
#   - https://kubernetes.io/docs/reference/generated/kubernetes-api/v1.12/#tokenreview-v1-authentication-k8s-io

Feature: Impersonating pod to decrypt it's secrets
  In order to decrypt pod's secrets
  As an attacker
  I want to impersonate this pod

  Scenario Outline: Impersonation via leaked token
    Given a service account token was compromised
    When the attacker use this token for authentication
    Then the attacker can decrypt this pod's secrets forever

    Examples: Data types
      | data-type         |
      | password          |
      | API key           |
      | X.509 private key |
      | SSH private key   |

  Scenario: Service accounts mount
    Given a permission to create a pod
    When the attacker launch a pod with anther pod's service account
    Then the attacker can use the token for authentication and decrypt the secrets
  
  Scenario: Impersonate Kubernetes API
    Given a compromised Kubernetes node
    And the attacker can spoof requests to Kubernetes API
    When Kamus use the API to review tokens
    Then the attacker can approve all requests
