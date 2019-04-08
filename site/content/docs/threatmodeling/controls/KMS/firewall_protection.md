# Enable firewall protection for KMS

Feature: Enable firewall protection for KMS
  In order to protect KMS 
  As an engineer
  I want to allow traffic to KMS only from Kamus

  Scenario: A KMS encrypt/decrypt request  
    Given firewall protection enabled on KMS
    When a request is sent not from Kamus
    Then the request will be denied

## Remarks

* Mitigates:
 * [Accessing KMS with leaked credentials](/docs/threatmodeling/threats/kms/leaked_credentials)
 * [Exposing names of namespaces and service accounts](/docs/threatmodeling/threats/encryption/namespace_enumeration)
* References:
 * https://docs.microsoft.com/en-us/azure/key-vault/key-vault-network-securit

Back to [Threats and Controls](/docs/threatmodeling/threats_controls)
