# Accessing KMS with leaked credentials

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

## Remarks

* Controls:
 * [Enable firewall protection for KMS](/docs/threatmodeling/controls/kms/firewall_protection)
 * [Credentials Hardening](/docs/threatmodeling/controls/kms/hardening_credentials)
 * [Use KMS with HSM support](/docs/threatmodeling/controls/kms/use_hsm)
*  References: 

Back to [Threats and Controls](/docs/threatmodeling/threats_controls)
