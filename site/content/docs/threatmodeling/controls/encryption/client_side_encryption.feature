# Id: KAMUS-C-E-1
# Status: Confirmed
# Components:
#   - Secrets encryption
# Mitigates:
#   - KAMUS-T-E-1
#   - KAMUS-T-E-3
# References:
#   - 

Feature: Client-side encryption
  In order to protect Kamus from DoS
  As an engineer
  I want to move all encryption logic to the client


  Scenario: Expose public key
    Given a user has aceess to the public key
    When the user need to encrypt a secret
    Then the user can encrypt it with the public key