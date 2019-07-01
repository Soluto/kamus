---
title: "Changelog"
menu:
  main:
    parent: "user"
    identifier: "changelog"
    weight: 6
---
# Changelog

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

- [**bug**] type=KamusSecret  is NOT working. [#196](https://github.com/Soluto/kamus/issues/196)
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
