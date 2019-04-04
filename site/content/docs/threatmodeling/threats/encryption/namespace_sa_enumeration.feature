# Id: KAMUS-T-E-2
# Status: Confirmed
# Components:
#   - Secret encryption
# STRIDE:
#   - Information Disclosure
# References:
#   - https://en.wikipedia.org/wiki/Denial-of-service_attack
#   - https://docs.microsoft.com/en-us/azure/key-vault/key-vault-service-limits


Feature: Exposing names of namespaces and service accounts
  In order to find information
  As an attacker
  I want to enumarate all possible combinations of namespaces and service accounts

  Scenario Outline: Enumeration of all possible combinations
    Given a list of all possible combinations
    And the attacker send encrypt request for each combination
    When the attacker receive sucess response
    Then the attacker know the combination exists on the cluster

    Examples: Data types
      | Namespace                |
      | Service account          |
      | Teams                    |
      | Partners                 |
      | Bussiness information    |