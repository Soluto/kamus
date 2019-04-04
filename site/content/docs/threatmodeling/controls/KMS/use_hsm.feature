# Id: KAMUS-C-K-2
# Status: Confirmed
# Components:
#   - Secrets decryption
#   - Secrets encryption
# Mitigates:
#   - KAMUS-T-K-1
# References:
#   - https://docs.microsoft.com/en-us/azure/key-vault/key-vault-hsm-protected-keys

Feature: Use KMS with HSM support
  In order to protect encryption keys 
  As an engineer
  I want to store them using HSM

  Scenario: Azure KeyVauls 
    Given Azure KeyVault keys created in HSM mode
    When a hacker tries to read the private key
    Then the request will be denied
