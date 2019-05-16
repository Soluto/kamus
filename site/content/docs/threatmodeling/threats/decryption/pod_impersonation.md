# Impersonating pod to decrypt it's secrets

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

## Remarks

* Controls:
 * [Deny request for default SA](/docs/threatmodeling/controls/decryption/deny_default_sa)
 * [Deny Kuberentes secrets view permissions](/docs/threatmodeling/controls/decryption/deny_secret_view)
 * [Use TLS when accessing Kuberentes API](/docs/threatmodeling/controls/decryption/k8s_api_tls)
 * [Use a policy to control pod's service account](/docs/threatmodeling/controls/decryption/opa_pods_secrets)
*  References: 
 * https://kubernetes.io/docs/tasks/configure-pod-container/configure-service-account/
 * https://kubernetes.io/docs/concepts/configuration/secret#service-accounts-automatically-create-and-attach-secrets-with-api-credentials
 * https://kubernetes.io/docs/reference/generated/kubernetes-api/v1.12/#tokenreview-v1-authentication-k8s-io

Back to [Threats and Controls](/docs/threatmodeling/threats_controls)