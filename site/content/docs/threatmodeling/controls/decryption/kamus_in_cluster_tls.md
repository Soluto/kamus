# Serve Kamus API over TLS

Feature: Serve Kamus API over TLS
  In order to protect Kamus from spoffing
  As an engineer
  I want to use TLS to encrypt traffic to and from Kamus

  Scenario: Using Kubernetes CA
    Given A certifcate from Kubernetes CA
    When Kamus serve a request
    Then The request will be encrypted using TLS


## Remarks

* Mitigates: KAMUS-T-D-2
* References:
 * https://stackoverflow.com/questions/50893535/securing-kubernetes-service-with-tls
 * https://istio.io/docs/tasks/security/https-overlay/

Back to [Threats and Controls](/docs/threatmodeling/threats_controls)
