# Using KamusSecret to decrypt secrets

Feature: Using KamusSecret to decrypt secrets
  In order to decrypt pod's secrets
  As an attacker
  I want to create KamusSecret

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

## Remarks

* Controls:
 * [Deny Kuberentes secrets view permissions](/docs/threatmodeling/controls/decryption/deny_secret_view)
*  References: 

Back to [Threats and Controls](/docs/threatmodeling/threats_controls)