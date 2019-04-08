# Sniffing user's traffic

Feature: Sniffing user's traffic
  In order to find information
  As an attacker
  I want to sniff user's traffic

  Scenario Outline: Sniffing encryption request
    Given a compromised user device
    When the user send an encryption request to Kamus
    Then the attacker can inspect the traffic and retrieve the secret sent to Kamus

    Examples: Data types
      | Namespace                |
      | Service account          |
      | Teams                    |
      | Partners                 |
      | Bussiness information    |

## Remarks

* Controls:
 * [Certificate pinning](/docs/threatmodeling/controls/encryption/certificate_pinning)
 * [Client-side encryption](/docs/threatmodeling/controls/encryption/client_side_encryption)
*  References: 
 * https://en.wikipedia.org/wiki/Man-in-the-middle_attack

Back to [Threats and Controls](/docs/threatmodeling/threats_controls)