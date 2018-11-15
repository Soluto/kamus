let program = require('commander');
const readfiles = require('node-readfiles');
const fs = require('fs');
const util = require('util');
const readFileAsync = util.promisify(fs.readFile);
const writeFile = util.promisify(fs.writeFile);
const axios = require('axios');

program
    .version('0.1.0')
    .option('-e, --encrypted-folder <path>', 'Encrypted files folder path')
    .option('-d, --decrypted-file <path>', 'Decrypted JSON path')
    .option('-f, --output-format <format>', 'The format of the output file, default to JSON. Supported types: json, cfg', /^(json|cfg)$/i, 'json')
    .parse(process.argv);

const getEncryptedFiles = async () => {
  return await readfiles(program.encryptedFolder, function (err, filename, contents) {
    if (err) throw err;
  });
}

const getKamusUrl = () => {
    let url = process.env.KAMUS_URL;
    if (!url) {
        throw new Error("Missing KAMUS_URL env var");
    }
    return url;
}

const getBarerToken = async () => {
    return await readFileAsync("/var/run/secrets/kubernetes.io/serviceaccount/token", "utf8");
}

const decryptFile = async (httpClient, filePath) => {
    var encryptedContent = await readFileAsync(program.encryptedFolder + '/' + filePath, "utf8");
    const response = await httpClient.post('/api/v1/decrypt', {data: encryptedContent});
    return response.data;
}

const serializeToCfgFormat = (secrets) => {
  var output = "";
  Object.keys(secrets).forEach(key => {
    output += `${key}=${secrets[key]}\n`
  });
  
  output = output.substring(0, output.lastIndexOf('\n'));

  return output;
}

async function innerRun() {

    console.log(program.outputFormat);
    let files = await getEncryptedFiles();
    let kamusUrl = getKamusUrl();
    let token = await getBarerToken();

    const httpClient = axios.create({
        baseURL: kamusUrl,
        timeout: 10000,
        headers: {"Content-Type": "application/json", "Authorization": "Bearer " + token}
    });

    let secrets = {};

    for (let file of files)
    {
        secrets[file] = await decryptFile(httpClient, file);
    }

    console.log(`Writing output format using ${program.outputFormat} format`);

    switch(program.outputFormat){
      case "json":
        await writeFile(program.decryptedFile, JSON.stringify(secrets));
        break;
      case "cfg":
        await writeFile(program.decryptedFile, serializeToCfgFormat(secrets));
        break;
    }
    
    
    
    console.log("Decrypted: " + Object.keys(secrets))
}

async function run() {
  try {
    await innerRun();
  }catch (e) {
    console.error("Failed to run init container: " + e);
    process.exit(1);
  }
}

run();