# Id: KAMUS-C-E-1
# Status: Confirmed
# Components:
#   - Secrets encryption
# Mitigates:
#   - KAMUS-T-E-1
#   - KAMUS-T-E-2
# References:
#   - https://kubernetes.io/docs/tasks/access-application-cluster/port-forward-access-application-cluster/

Feature: Block anonymous internet access to Kamus
  In order to protect Kamus from DoS
  As an engineer
  I want to block anonymous access to Kamus from the internet


  Scenario: Use Kubernetes port-forward
    Given a user has valid Kubernetes config file
    When the user open port forward to Kamus service
    Then the user can use Kamus for encryption

  Scenario: User authentication
    Given an authenticated user (using Kubernetes user token?)
    When the user try to encrypt a secret
    Then the user is allowed to perform the request
