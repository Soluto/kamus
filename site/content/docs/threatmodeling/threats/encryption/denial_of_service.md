# Kamus server disrupting

Feature: Kamus server disrupting
  In order to distrupt Kamus server
  As an attacker
  I want to load it with requests

  Scenario: Encryption requests
    Given a name of a namespace and a service account
    When the attacker send massive amount of encrypt request
    Then the attacker can distrupt the service

## Remarks

* Controls:
 * [Block anonymous internet access to Kamus](/docs/threatmodeling/controls/encryption/block_internet_access)
 * [Deny request for default SA](/docs/threatmodeling/controls/encryption/deny_default_sa)
 * [Client-side encryption](/docs/threatmodeling/controls/encryption/client_side_encryption)
 * [Throttling requests by source IP](/docs/threatmodeling/controls/encryption/ip_throttling)
*  References: 
 * https://en.wikipedia.org/wiki/Denial-of-service_attack
 * https://docs.microsoft.com/en-us/azure/key-vault/key-vault-service-limits

Back to [Threats and Controls](/docs/threatmodeling/threats_controls)