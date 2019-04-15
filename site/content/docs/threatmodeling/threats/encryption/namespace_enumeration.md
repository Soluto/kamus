# Exposing names of namespaces and service accounts

Feature: Exposing names of namespaces and service accounts
  In order to find information
  As an attacker
  I want to enumarate all possible combinations of namespaces and service accounts

  Scenario Outline: Enumeration of all possible combinations
    Given a list of all possible combinations
    And the attacker send encrypt request for each combination
    When the attacker receive sucess response
    Then the attacker know the combination exists on the cluster

    Examples: Data types
      | Namespace                |
      | Service account          |
      | Teams                    |
      | Partners                 |
      | Bussiness information    |

## Remarks

* Controls:
 * [Block anonymous internet access to Kamus](/docs/threatmodeling/controls/encryption/block_internet_access)
 * [Deny request for default SA](/docs/threatmodeling/controls/encryption/deny_default_sa)
 * [Enable firewall protection for KMS](/docs/threatmodeling/controls/kms/firewall_protection)
*  References: 
 * https://en.wikipedia.org/wiki/Denial-of-service_attack
 * https://docs.microsoft.com/en-us/azure/key-vault/key-vault-service-limits

Back to [Threats and Controls](/docs/threatmodeling/threats_controls)