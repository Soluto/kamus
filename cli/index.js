#!/usr/bin/env node

const pjson = require('./package.json');
const prog = require('caporal');
const encrypt = require('./actions/encrypt');
const regexGuid = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

prog
  .version(pjson.version)
  .command('encrypt', 'Encrypt data')
  .action(encrypt)
  .option('-s, --secret <secret>','Secret to encrypt', prog.STRING)
  .option('-f, --secret-file <file>','File with secret to encrypt', prog.STRING)
  .option('-a, --service-account <service-account>', 'Deployment service account', prog.REQUIRED)
  .option('-n, --namespace <namespace>', 'Deployment namespace', prog.REQUIRED)
  .option('-u, --kamus-url <kamusUrl>', 'Kamus URL', prog.REQUIRED)
  .option('--auth-tenant <id>', 'Azure tenant id', regexGuid)
  .option('--auth-application <id>', 'Azure application id', regexGuid)
  .option('--auth-resource <name>', 'Azure resource name', prog.STRING)
  .option('--allow-insecure-url', 'Allow insecure (http) Kamus URL', prog.BOOL)
  .option('--cert-fingerprint <certFingerprint>', 'Force server certificate to match the given fingerprint', prog.STRING)
  .option('--secret-file-encoding <fileEncoding>', 'Encoding of secret file', prog.STRING);

prog.parse(process.argv);