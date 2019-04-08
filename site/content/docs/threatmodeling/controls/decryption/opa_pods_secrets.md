# Use a policy to control pod's service account

Feature: Use a policy to control pod's service account
  In order to protect pods from impersonation
  As a cluster admin
  I want to use a policy to define which pods can use which service account

  Scenario: Using Azure OPA
    Given a pod A
    And a service account B is used by another pod B
    When the user try mount service account B to pod A
    Then the operation denied

  Scenario: Using Kubernetes addmission controller
    Given a pod A
    And a service account B is used by another pod B
    When the user try mount service account B to pod A
    Then the operation denied


## Remarks

* Mitigates:
 * [Impersonating pod to decrypt it's secrets](/docs/threatmodeling/threats/decryption/pod_impersonation)
 * [Sniff requests and responses to Kamus](/docs/threatmodeling/threats/decryption/sniffing_tampering)
* References:
 * https://stackoverflow.com/questions/50893535/securing-kubernetes-service-with-tls
 * https://istio.io/docs/tasks/security/https-overlay/

Back to [Threats and Controls](/docs/threatmodeling/threats_controls)
