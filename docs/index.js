var Gherkin = require('gherkin');
var fs = require('fs');
var parser = new Gherkin.Parser();

var output = parser.parse(fs.readFileSync('/Users/omerl/dev/Kamus/docs/features/threats/decryption/pod_impersonation.feature', 'utf8'));

console.log(output);