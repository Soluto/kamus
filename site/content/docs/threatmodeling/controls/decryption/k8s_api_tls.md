# Use TLS when accessing Kuberentes API

Feature: Use TLS when accessing Kuberentes API
  In order to protect Kamus from spoffing
  As an engineer
  I want to use TLS for all requests to Kubernetes API

  Scenario: Using token review API
    Given Kamus request to token review API
    And The request is not using TLS
    When sending the requesrt
    Then the request will fail


## Remarks

* Mitigates: 
 * [Impersonating pod to decrypt it's secrets](/docs/threatmodeling/threats/decryption/pod_impersonation)

Back to [Threats and Controls](/docs/threatmodeling/threats_controls)
