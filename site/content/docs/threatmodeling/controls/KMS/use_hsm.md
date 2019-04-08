# Use KMS with HSM support

Feature: Use KMS with HSM support
  In order to protect encryption keys 
  As an engineer
  I want to store them using HSM

  Scenario: Azure KeyVauls 
    Given Azure KeyVault keys created in HSM mode
    When a hacker tries to extract the private key
    Then the request will be denied

## Remarks

* Mitigates:
 * [Accessing KMS with leaked credentials](/docs/threatmodeling/threats/kms/leaked_credentials)
* References:
 * https://docs.microsoft.com/en-us/azure/key-vault/key-vault-hsm-protected-keys

Back to [Threats and Controls](/docs/threatmodeling/threats_controls)