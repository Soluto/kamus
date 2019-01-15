#!/usr/bin/env node

var pjson = require('./package.json');
const prog = require('caporal');
const encrypt = require('./actions/encrypt');
const regexGuid = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

prog
  .version(pjson.version)
  .command('encrypt', 'Encrypt data')
  .action(encrypt)
  .option('--secret <data>','Data to encrypt', prog.REQUIRED)
  .option('--service-account <service-account>', 'Deployment service account', prog.REQUIRED)
  .option('--namespace <namespace>', 'Deployment namespace', prog.REQUIRED)
  .option('--kamus-api-url <url>', 'Kamus URL', prog.REQUIRED)
  .option('--auth-tenant <id>', 'Azure tenant id', regexGuid)
  .option('--auth-application <id>', 'Azure application id', regexGuid)
  .option('--auth-resource <name>', 'Azure resource name', prog.STRING)
  .option('--allow-insecure-url', 'Allow insecure (http) Kamus URL', prog.BOOL)
  .option('--cert-fingerprint <certFingerprint>', 'Force server certificate to match the given fingerprint', prog.STRING);

prog.parse(process.argv);