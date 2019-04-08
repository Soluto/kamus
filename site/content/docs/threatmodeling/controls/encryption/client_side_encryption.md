# Client-side encryption

Feature: Client-side encryption
  In order to protect Kamus from DoS
  As an engineer
  I want to move all encryption logic to the client

  Scenario: Expose public key
    Given a user has aceess to the public key
    When the user need to encrypt a secret
    Then the user can encrypt it with the public key
## Remarks

* Mitigates:
 * [Kamus server disrupting](/docs/threatmodeling/threats/encryption/denial_of_service)
 * [Sniffing user's traffic](/docs/threatmodeling/threats/encryption/sniffing_user_traffic)
* Status: proposed. Currently we decided it's better to perform all encryption in the server side for simplicity.

Back to [Threats and Controls](/docs/threatmodeling/threats_controls)