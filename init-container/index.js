let program = require('commander');
const readfiles = require('node-readfiles');
const fs = require('fs');
const util = require('util');
const readFileAsync = util.promisify(fs.readFile);
const writeFile = util.promisify(fs.writeFile);
const axios = require('axios');
const path = require('path');

program
    .version('0.1.0')
    .option('-e, --encrypted-folder <path>', 'Encrypted files folder path')
    .option('-d, --decrypted-path <path>', 'Decrypted file/s folder path')
    .option('-n, --decrypted-file-name <name>', 'Decrypted file name' )
    .option('-f, --output-format <format>', 'The format of the output file, default to JSON. Supported types: json, cfg, files', /^(json|cfg|files)$/i, 'json')
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
    try {
      const response = await httpClient.post('/api/v1/decrypt', {data: encryptedContent});
      return response.data;
    } catch (e) {
      throw new Error(`request to decrypt API failed: ${e.response ? e.response.status : error.message}`)
    }
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

const serializeToCfgFormatStrict = (secrets) => {
  var output = "";
  Object.keys(secrets).forEach(key => {
    switch(typeof(secrets[key]))
    {
      case "string":
        output += `${key}="${secrets[key]}"\n`
        break;
      default:
        output += `${key}=${secrets[key]}\n`
    }
    
  });
  
  output = output.substring(0, output.lastIndexOf('\n'));

  return output;
}

async function innerRun() {

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
    
    const outputFile = path.join(program.decryptedPath, program.decryptedFileName);
    console.log(`Writing output format using ${program.outputFormat} format to file ${outputFile}`);

    switch(program.outputFormat.toLowerCase()){
      case "json":
        await writeFile(outputFile, JSON.stringify(secrets));
        break;
      case "cfg":
        await writeFile(outputFile, serializeToCfgFormat(secrets));
        break;
      case "cfg-strict":
        await writeFile(outputFile, serializeToCfgFormatStrict(secrets));
        break;
      case "files":
        await Promise.all(Object.keys(secrets).map(secretName => writeFile(path.join(program.decryptedPath, secretName), secrets[secretName])))
        break;
      default:
        throw new Error(`Unsupported output format: ${program.outputFormat}`);
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
