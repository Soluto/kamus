# Id: KAMUS-C-D-2
# Status: Confirmed
# Components:
#   - Secrets decryption
# Mitigates:
#   - KAMUS-T-D-1
# References:
#   - https://github.com/Azure/kubernetes-policy-controller

Feature: Use a policy to control pod's service account
  In order to protect pods from impersonation
  As a cluster admin
  I want to use a policy to define which pods can use which service account

  Scenario: Using Azure OPA
    Given a pod A
    And a service account B is used by another pod B
    When the user try mount service account B to pod A
    Then the operation denied

  Scenario: Using Kubernetes addmission controller
    Given a pod A
    And a service account B is used by another pod B
    When the user try mount service account B to pod A
    Then the operation denied
