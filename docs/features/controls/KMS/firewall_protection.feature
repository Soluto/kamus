# Id: KAMUS-C-K-1
# Status: Confirmed
# Components:
#   - Secrets decryption
#   - Secrets encryption
# Mitigates:
#   - KAMUS-T-K-1
# References:
#   - https://docs.microsoft.com/en-us/azure/key-vault/key-vault-network-security

Feature: Enable firewall protection for KMS
  In order to protect KMS 
  As an engineer
  I want to allow traffic to KMS only from Kamus

  Scenario: A KMS encrypt/decrypt request  
    Given firewall protection enabled on KMS
    When a request is sent not from Kamus
    Then the request will be denied
