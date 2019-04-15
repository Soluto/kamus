# Sniff requests and responses to Kamus

Feature: Sniff requests and responses to Kamus
  In order to compromise secrets 
  As an attacker
  I want to sniff requests and response to Kamus

  Scenario Outline: MitM
    Given a compromised Kubernetes node
    When the attacker sniff all the network traffic in this node
    Then the attacker can view reqeusts and responses to Kamus

    Examples: Data types
      | data-type                |
      | password                 |
      | API key                  |
      | X.509 private key        |
      | SSH private key          |
      | Service account toekns   |

## Remarks

* Controls:
 * [Serve Kamus API over TLS](/docs/threatmodeling/controls/decryption/kamus_in_cluster_tls)
 * [Use a policy to control pod's service account](/docs/threatmodeling/controls/decryption/opa_pods_secrets)
*  References: 
 * https://en.wikipedia.org/wiki/Man-in-the-middle_attack

Back to [Threats and Controls](/docs/threatmodeling/threats_controls)