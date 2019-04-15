# Key names obfuscation

Feature: Key names obfuscation
  In order to protect encryption keys 
  As an engineer
  I want to obfuscate the key names

  Scenario: Using SHA256
    Given a namespace and service account
    When Kamus access KMS to retrieve the encryption key
    Then an obfuscated name will be created from the namespace and service account using SHA256


## Remarks

* Mitigates:
* Status: Partial implementation. In future version, we'll add support to add a salt.

Back to [Threats and Controls](/docs/threatmodeling/threats_controls)