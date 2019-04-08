# Throttling requests by source IP

Feature: Throttling requests by source IP
  In order to protect Kamus from DoS
  As an engineer
  I want to throttle incoming requests

  Scenario: Using Nginx Ingress
    Given an attacker sending multiple requests
    When the limit of allowed request breached 
    Then Nginx will deny these requests from hitting Kamus

## Remarks

* Mitigates:
 * [Kamus server disrupting](/docs/threatmodeling/threats/encryption/denial_of_service)
 * [Exposing names of namespaces and service accounts](/docs/threatmodeling/threats/encryption/namespace_enumeration)
* References:
 * https://github.com/Shopify/ingress/blob/master/docs/user-guide/nginx-configuration/annotations.md#rate-limiting

Back to [Threats and Controls](/docs/threatmodeling/threats_controls)
