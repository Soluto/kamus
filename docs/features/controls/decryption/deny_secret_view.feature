# Id: KAMUS-C-D-1
# Status: Confirmed
# Components:
#   - Secrets decryption
# Mitigates:
#   - KAMUS-T-D-1
#   - KAMUS-T-D-3
# References:
#   - https://kubernetes.io/docs/reference/access-authn-authz/rbac/

Feature: Deny Kuberentes secrets get permissions
  In order to protect Kamus from DoS
  As a cluster admin
  I want to deny all users from getting Kubernetes secrets in all namespaces

  Scenario: Using Kubernetes RBAC
    Given a user role without secrets get permissions 
    When the user try to read service account's secret
    Then the operation denied
