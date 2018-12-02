# Id: KAMUS-T-D-2
# Status: Confirmed
# Components:
#   - Secret decryption
# STRIDE:
#   - Spoofing
#   - Information Disclosure 
# References:
#   - https://en.wikipedia.org/wiki/Man-in-the-middle_attack


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