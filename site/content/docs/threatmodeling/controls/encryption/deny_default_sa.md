# Deny request for default SA

Feature: Deny request for default SA
  In order to protect enumeration and make it harder to perform DoS attack
  As an engineer
  I want to deny all requests with default service account

  Scenario: An encrypt request for default SA 
    Given a default service acount
    When the user try to encrypt data for this service account
    Then the operation denied

## Remarks

* Mitigates:
 * [Kamus server disrupting](/docs/threatmodeling/threats/encryption/denial_of_service)
 * [Exposing names of namespaces and service accounts](/docs/threatmodeling/threats/encryption/namespace_enumeration)

Back to [Threats and Controls](/docs/threatmodeling/threats_controls)
