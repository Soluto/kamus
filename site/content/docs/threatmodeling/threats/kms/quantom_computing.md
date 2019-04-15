# Breaking encryption key

Feature: Breaking encryption key
  In order to decrypt pod's secrets
  As an attacker
  I want to find the private key from the public key

  Scenario Outline: Using Shor's algorithm
    Given a client with acess to a public key used by Kamus
    And a quantom-powered computer
    When the attacker use Shor's algorithm
    Then the attacker can find the matching private key

    Examples: Data types
      | data-type         |
      | password          |
      | API key           |
      | X.509 private key |
      | SSH private key   |

## Remarks

* Controls:
*  References: 
 * https://en.wikipedia.org/wiki/Shor%27s_algorithm

Back to [Threats and Controls](/docs/threatmodeling/threats_controls)