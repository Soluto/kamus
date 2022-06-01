---
title: "Changelog"
menu:
  main:
    parent: "user"
    identifier: "changelog"
    weight: 6
---

# Changelog

## kamus-0.9.0.8 (18/03/2022)

#### chore :

- Upgrade base images to address security vulnerabilities

## kamus-0.9.0.7 (18/03/2021)

#### feature :

- The controller now reconcile all KamusSecrets every 60 seconds (make sure to recreate if any secret is missing)

## kamus-0.9.0.6 (15/02/2021)

#### feature :

- Container images moved to ghcr

## kamus-0.9.0.5 (15/02/2021)

#### bug :

- Prevent controller restarts every 60 minutes

## kamus-0.9.0.2 (14/02/2021)

#### bug :

- Fix crd controller logging

## kamus-0.9.0.1 (14/02/2021)

#### chore :

- Remove SSL endpoint since we don't have conversion webhook anymore

## kamus-0.9 (11/02/2021)

#### Breaking :

- Kubernetes 1.16 is the minimum required version since KamusSecret CRD moved to `apiextensions.k8s.io/v1`
- v1alpha1 KamusSecret was removed. Please migrate to v1alpha2.

  To migrate from v1alpha1 to v1alpha2 all you need to do is:

  - Change the key data to stringData
  - Change the apiVersion to "soluto.com/v1alpha2"

## kamus-0.8 (31/08/2020)

#### Breaking Bug Fixes:

- AwsKeyManagement didn't use `cmkPrefix` parameter which failed decryption requests of previous versions (< 0.7.0.0) encrypted secrets.

  If you've encrypted secrets using kamus 0.7.0.0 and above, your AWS key aliases were created without prefix, so you have to re-encrypt them in order to use kamus 0.8.0.0 that now uses the cmkPrefix correctly.

## kamus-0.7 (07/06/2020)

#### Bug Fixes:

- [**bug**] Decryption doesn't fail when doing from unauthorized service account [#526](https://github.com/Soluto/kamus/issues/526)

## kamus-0.6 (19/09/2019)

#### Enhancements:

- [**enhancement**] Unable to add binary file to KamusSecret [#246](https://github.com/Soluto/kamus/issues/246)
- [**enhancement**] Support Key Rolling [#23](https://github.com/Soluto/kamus/issues/23)

#### Bug Fixes:

- [**bug**] BUG: init container -n flag required when outputting files format [#270](https://github.com/Soluto/kamus/issues/270)
- [**bug**] GCP KMS support is broken since version 0.4.4.0 [#251](https://github.com/Soluto/kamus/issues/251)

## The released can be used using the latest chart version - 0.4.0

## kamus-0.5 (30/06/2019)

#### Enhancements:

- [**enhancement**] Drop testing against Kubernetes 1.12 and start testing against Kubernetes 1.15 [#223](https://github.com/Soluto/kamus/pull/223)
- [**enhancement**] Add CRD to create secrets wrapper [#13](https://github.com/Soluto/kamus/issues/13)
- [**enhancement**] Add support for modifying KamusSecret [#225](https://github.com/Soluto/kamus/issues/225)
- [**documentation**] document method of installing kamus without Helm [#215](https://github.com/Soluto/kamus/issues/215)
- [**documentation**] Document decryption flow [#207](https://github.com/Soluto/kamus/issues/207)
- [**enhancement**][**good first issue**][**help wanted**] Ability to encrypt only values that are actually secret [#202](https://github.com/Soluto/kamus/issues/202)
- [**documentation**] The documentation has incorrect reference of "KeyManager" [#197](https://github.com/Soluto/kamus/issues/197)
- [**documentation**] encryptor can't work with kms provider via kiam role [#158](https://github.com/Soluto/kamus/issues/158)
- [**chart**][**documentation**] Document POD annotations [#151](https://github.com/Soluto/kamus/issues/151)
- [**enhancement**][**good first issue**][**help wanted**] init container should accept multiple encrypted directories [#139](https://github.com/Soluto/kamus/issues/139)

#### Bug Fixes:

- [**bug**] type=KamusSecret is NOT working. [#196](https://github.com/Soluto/kamus/issues/196)
- [**bug**] CRD Controller fails to decrypt [#163](https://github.com/Soluto/kamus/issues/163)
- [**bug**] getting error when trying to encrypt with awskms [#142](https://github.com/Soluto/kamus/issues/142)
- [**bug**] Failed to run the example when Kamus is not running in the default namespace [#132](https://github.com/Soluto/kamus/issues/132)
- [**bug**] Error using --verbose with kamus-cli: [TypeError: Cannot convert undefined or null to object] [#131](https://github.com/Soluto/kamus/issues/131)

---

## kamus-0.3 (07/03/2019)

#### Enhancements:

- [**enhancement**][**help wanted**] Add support for AWS KMS [#60](https://github.com/Soluto/kamus/issues/60)

#### Bug Fixes:

- [**bug**] String not formatted properly when using cfg format [#117](https://github.com/Soluto/kamus/issues/117)
- [**bug**][**good first issue**] Fix DAST [#46](https://github.com/Soluto/kamus/issues/46)

Available on chart version 0.1.7

---

## kamus-0.2 (07/03/2019)

#### Enhancements:

- [**enhancement**] Init container errors are not informative [#103](https://github.com/Soluto/kamus/issues/103)
- [**enhancement**] Configure eslint for CLI [#99](https://github.com/Soluto/kamus/issues/99)
- [**enhancement**] Colorized CLI logs [#97](https://github.com/Soluto/kamus/issues/97)
- [**enhancement**][**good first issue**][**help wanted**] Add option to output to file [#84](https://github.com/Soluto/kamus/issues/84)
- [**enhancement**] Add support for file input for <data> arg [#35](https://github.com/Soluto/kamus/issues/35)

#### Bug Fixes:

- [**bug**] Secret argument doesn't been pass to API [#95](https://github.com/Soluto/kamus/issues/95)
- [**bug**] Kamus API should return 400 on invalid request [#93](https://github.com/Soluto/kamus/issues/93)
- [**bug**] Bug - decryptor api is Alive return 500 [#91](https://github.com/Soluto/kamus/issues/91)
- [**bug**] Fix CLI CI tests for PR [#89](https://github.com/Soluto/kamus/issues/89)

---

## kamus-0.1 (17/01/2019)

- [**enhancement**][**good first issue**][**help wanted**] Make CLI arguments order invariant [#77](https://github.com/Soluto/kamus/issues/77)
- [**enhancement**] Move to versioned docker images [#74](https://github.com/Soluto/kamus/issues/74)
- [**enhancement**][**help wanted**] Add support for GCP KMS [#61](https://github.com/Soluto/kamus/issues/61)
- [**enhancement**] Support envelope encryption [#26](https://github.com/Soluto/kamus/issues/26)
- [**enhancement**] In-Memory key provider [#17](https://github.com/Soluto/kamus/issues/17)
- [**bug**] Decryptor prints decrypted values to logs [#11](https://github.com/Soluto/kamus/issues/11)
- [**enhancement**] Create CLI [#10](https://github.com/Soluto/kamus/issues/10)
- [**enhancement**] Add support for init container [#9](https://github.com/Soluto/kamus/issues/9)
- [**enhancement**] Tests for decryptor utility [#5](https://github.com/Soluto/kamus/issues/5)
- [**enhancement**] Audit logs [#1](https://github.com/Soluto/kamus/issues/1)
