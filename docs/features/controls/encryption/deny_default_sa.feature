# Id: KAMUS-C-D-3
# Status: Confirmed
# Components:
#   - Secrets decryption
# Mitigates:
#   - KAMUS-T-E-1
#   - KAMUS-T-E-2
# References:
#   - 

Feature: Deny request for default SA
  In order to protect enumeration and make it harder to perform DoS attack
  As an engineer
  I want to deny all requests with default service account

  Scenario: An encrypt request for default SA 
    Given a default service acount
    When the user try to encrypt data for this service account
    Then the operation denied
