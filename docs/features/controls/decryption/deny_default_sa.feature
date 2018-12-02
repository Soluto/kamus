# Id: KAMUS-C-D-5
# Status: Confirmed
# Components:
#   - Secrets decryption
# Mitigates:
#   - KAMUS-T-D-1
# References:
#   - https://kubernetes.io/docs/reference/access-authn-authz/rbac/

Feature: Deny request for default SA
  In order to protect pods from impersonation
  As an engineer
  I want to deny all requests with default service accounts 

  Scenario: A decrypt request with token for default SA 
    Given a pod that is mounted with default service account
    When the pod try to decrypt secrets using the token
    Then the operation denied
