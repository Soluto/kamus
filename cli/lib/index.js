#!/usr/bin/env node

const pjson = require('../package.json');
const prog = require('caporal');
const encrypt = require('./actions/encrypt');
const regexGuid = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

const { ColorfulChalkLogger, INFO } = require('colorful-chalk-logger');

// translate to ColorfulChalkLogger log level
if (process.argv.indexOf('--verbose')) {
  process.argv = process.argv.filter(arg => arg != '--verbose');
  process.argv.push('--log-level');
  process.argv.push('debug');
}

const logger = new ColorfulChalkLogger('kamus-cli', {
  level: INFO,
  date: false,
  colorful: true, 
}, process.argv);

// remove the log levels before running caporal
process.argv = process.argv.filter(x => (x != '--log-level' && x != 'debug'));

prog
  .logger(logger)
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
  .option('--secret-file-encoding <fileEncoding>', 'Encoding of secret file', prog.STRING)
  .option('-o, --output <outputFile>', 'Output to file', prog.STRING)
  .option('-O, --overwrite', 'Overwrites file if it already exists', prog.BOOL)
  .option('--log-flag <date|inline|colorful|no-date|no-inline|no-colorful>', 'log format', prog.STRING)
  .option('--log-output <filepath>', 'output log to file', prog.STRING)
  .option('--log-encoding <encoding>', 'log file encoding');

prog.parse(process.argv);