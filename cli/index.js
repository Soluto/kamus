var pjson = require('./package.json');
const prog = require('caporal');
const encrypt = require('./encrypt');
const regexGuid = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

prog
  .version(pjson.version)
  .command('encrypt', 'Encrypt data')
    .argument('<data>','Data to encrypt')
    .argument('<service-account>', 'Deployment service account')
    .argument('<namespace>', 'Deployment namespace')
    .action(encrypt)
  .option('--auth-tenant <id>', 'Azure tenant id', regexGuid)
  .option('--auth-application <id>', 'Azure application id', regexGuid)
  .option('--auth-resource <name>', 'Azure resource name', prog.STRING)
  .option('--kamus-url <url>', 'Kamus URL', prog.REQUIRED);

prog.parse(process.argv);