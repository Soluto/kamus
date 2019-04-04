# Id: KAMUS-C-K-4
# Status: Confirmed
# Components:
#   - Secrets decryption
#   - Secrets encryption
# Mitigates:
#   - KAMUS-T-K-1
# References:
#   - 

Feature: Key names obfuscation
  In order to protect encryption keys 
  As an engineer
  I want to obfuscate the key names

  Scenario: Using SHA256
    Given a namespace and service account
    When Kamus access KMS to retrieve the encryption key
    Then an obfuscated name will be created from the namespace and service account using SHA256
