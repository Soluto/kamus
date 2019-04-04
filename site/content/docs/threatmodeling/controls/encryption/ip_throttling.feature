# Id: KAMUS-C-E-1
# Status: Confirmed
# Components:
#   - Secrets encryption
# Mitigates:
#   - KAMUS-T-E-1
#   - KAMUS-T-E-2
# References:
#   - https://github.com/Shopify/ingress/blob/master/docs/user-guide/nginx-configuration/annotations.md#rate-limiting

Feature: Throttling requests by source IP
  In order to protect Kamus from DoS
  As an engineer
  I want to throttle incoming requests

  Scenario: Using Nginx Ingress
    Given an attacker sending multiple requests
    When the limit of allowed request breached 
    Then Nginx will deny these requests from hitting Kamus
