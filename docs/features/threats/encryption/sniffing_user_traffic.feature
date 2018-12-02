# Id: KAMUS-T-E-3
# Status: Confirmed
# Components:
#   - Secret encryption
# STRIDE:
#   - Spoofing
#   - Information Disclosure 
# References:
#   - https://en.wikipedia.org/wiki/Man-in-the-middle_attack

Feature: Sniffing user's traffic
  In order to find information
  As an attacker
  I want to sniff user's traffic

  Scenario Outline: Sniffing encryption request
    Given a compromised user device
    When the user send an encryption request to Kamus
    Then the attacker can inspect the traffic and retrieve the secret sent to Kamus

    Examples: Data types
      | Namespace                |
      | Service account          |
      | Teams                    |
      | Partners                 |
      | Bussiness information    |