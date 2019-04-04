# Id: KAMUS-C-E-5
# Status: Confirmed
# Components:
#   - Secrets encryption
# Mitigates:
#   - KAMUS-T-E-3
# References:
#   - https://www.owasp.org/index.php/Certificate_and_Public_Key_Pinning

Feature: Certificate pinning
  In order to protect Kamus from DoS
  As an engineer
  I want to protect the user from MitM attack


  Scenario: Certificate pinning
    Given a user that encrypt a secret using Kamus CLI
    When the user initiate a TLS session with Kamus
    Then the server certificate is validated with a pre-defined certificate