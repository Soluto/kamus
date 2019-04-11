# Id: KAMUS-T-D-3
# Status: Confirmed
# Components:
#   - Secret decryption
# STRIDE:
#   - Spoofing
#   - Information Disclosure 
# References:
#   - https://kubernetes.io/docs/reference/access-authn-authz/rbac/

Feature: Using CRD to decrypt secrets
  In order to decrypt pod's secrets
  As an attacker
  I want to leverage KamusSecret CRD

  Scenario Outline: Creating KamusSecret
    Given a user with permissions to create KamusSecret and get Kubernetes Secrets
    When the user create a KamusSecret with a pod's secrets from the same namespace
    Then the user can get the created Kubernetes Secret and read the decrypted secrets

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
