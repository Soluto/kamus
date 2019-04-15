# Certificate pinning

Feature: Certificate pinning
  In order to protect Kamus from DoS
  As an engineer
  I want to protect the user from MitM attack


  Scenario: Certificate pinning
    Given a user that encrypt a secret using Kamus CLI
    When the user initiate a TLS session with Kamus
    Then the server certificate is validated with a pre-defined certificate

## Remarks

* Mitigates:
 * [Sniffing user's traffic](/docs/threatmodeling/threats/encryption/sniffing_user_traffic)
* References:
 * https://www.owasp.org/index.php/Certificate_and_Public_Key_Pinning
 * TODO: link to cert pinning docs in the CLI

Back to [Threats and Controls](/docs/threatmodeling/threats_controls)