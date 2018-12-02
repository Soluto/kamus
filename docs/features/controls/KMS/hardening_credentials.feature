# Id: KAMUS-C-K-3
# Status: Confirmed
# Components:
#   - Secrets decryption
#   - Secrets encryption
# Mitigates:
#   - KAMUS-T-K-1
# References:

Feature: Credentials Hardening
  In order to protect KMS 
  As an engineer
  I want to harden the credentials used by Kamus

  Scenario: Short-lived credentials
    When I create KMS credentials 
    Then I will use as short as possible expiration time

  Scenario: Use machine identity for authentication
    Given A machine idetity support is available
    When Kamus authenticate using this identity
    Then The request succeed
