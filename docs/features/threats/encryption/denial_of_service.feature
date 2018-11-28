# Id: KAMUS-T-E-1
# Status: Confirmed
# Components:
#   - Secret encryption
# STRIDE:
#   - DoS
# References:
#   - https://en.wikipedia.org/wiki/Denial-of-service_attack
#   - https://docs.microsoft.com/en-us/azure/key-vault/key-vault-service-limits


Feature: Kamus server disrupting
  In order to distrupt Kamus server
  As an attacker
  I want to load it with requests

  Scenario: Encryption requests
    Given a name of a namespace and a service account
    When the attacker send massive amount of encrypt request
    Then the attacker can distrupt the service