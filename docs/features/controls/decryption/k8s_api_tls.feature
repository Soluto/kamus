# Id: KAMUS-C-D-3
# Status: Confirmed
# Components:
#   - Secrets decryption
# Mitigates:
#   - KAMUS-T-D-1
# References:
#   - TBD

Feature: Use TLS when accessing Kuberentes API
  In order to protect Kamus from spoffing
  As an engineer
  I want to use TLS for all requests to Kubernetes API

  Scenario: Using token review API
    Given Kamus request to token review API
    And The request is not using TLS
    When sending the requesrt
    Then the request will fail
