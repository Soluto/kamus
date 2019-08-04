let program = require('commander');
const readfiles = require('node-readfiles');
const fs = require('fs');
const util = require('util');
const readFileAsync = util.promisify(fs.readFile);
const writeFile = util.promisify(fs.writeFile);
const axios = require('axios');
const path = require('path');
let ejs = require('ejs');

program
    .version('0.1.0')
    .option('-e, --encrypted-folders <path>', 'Encrypted files folder paths, comma separated')
    .option('-d, --decrypted-path <path>', 'Decrypted file/s folder path')
    .option('-n, --decrypted-file-name <name>', 'Decrypted file name' )
    .option('-f, --output-format <format>', 'The format of the output file, default to JSON. Supported types: json, cfg, files, custom', /^(json|cfg|cfg-strict|files|custom)$/i, 'json')
    .parse(process.argv);

//Source: https://blog.raananweber.com/2015/12/15/check-if-a-directory-exists-in-node-js/
function checkDirectorySync(directory) {  
  try {
    fs.statSync(directory);
  } catch(e) {
    fs.mkdirSync(directory);
  }
}
    

const getEncryptedFiles = async (folder) => {
    return await readfiles(folder, function (err, filename, contents) {
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
    var tokenFilePath = process.env.TOKEN_FILE_PATH;
    if (tokenFilePath == null || tokenFilePath == "")
    {
      tokenFilePath = "/var/run/secrets/kubernetes.io/serviceaccount/token";
    }
    return await readFileAsync(tokenFilePath, "utf8");
}

const stringifyIfJson = (secretValue) =>{
  return((typeof secretValue === "object") ? JSON.stringify(secretValue) : secretValue);
}

const decryptFile = async (httpClient, filePath, folder) => {
    console.log(`Decrypting ${filePath}`);
    var encryptedContent = await readFileAsync(folder + '/' + filePath, "utf8");
    try {
      const response = await httpClient.post('/api/v1/decrypt', {data: encryptedContent});
      return response.data;
    } catch (e) {
      throw new Error(`request to decrypt API failed: ${e.response ? e.response.status : e.message}`)
    }
}

const writeFileWithTemplate = async (secrets, templateName, outputFile) => {
  var template = await readFileAsync(templateName, "utf-8");
  var rendered = ejs.render(template, {secrets, stringifyIfJson}, {});
  await writeFile(outputFile, rendered);
}

async function innerRun() {

    let kamusUrl = getKamusUrl();
    let token = await getBarerToken();
    const httpClient = axios.create({
        baseURL: kamusUrl,
        timeout: 10000,
        headers: {"Content-Type": "application/json", "Authorization": "Bearer " + token}
    });

    let secrets = {};
    var templatePath = "";
    for (let folder of program.encryptedFolders.split(",")) {
        let files = await getEncryptedFiles(folder);
        for (let file of files) {
            if (file === "template.ejs" && program.outputFormat === "custom"){
              templatePath = path.join(folder, file)
              continue;
            }
            secrets[file] = await decryptFile(httpClient, file, folder);
        }
    }

    if (!program.decryptedPath)
    {
      throw "decrypted path wasn't provided although it's mandatory";
    }

    checkDirectorySync(program.decryptedPath);

    const outputFormat = program.outputFormat.toLowerCase();
    let outputFile = "";

    if (!program.decryptedFileName) 
    {
      if(outputFormat != "files"){
        throw "decrypted file name wasn't provided altough it's mandataroy";
      }
      console.log(`Writing output format using ${program.outputFormat} format to file ${program.decryptedPath}`);
    } else {
      outputFile = path.join(program.decryptedPath, program.decryptedFileName);
      console.log(`Writing output format using ${program.outputFormat} format to file ${outputFile}`);
    }
     

    switch(program.outputFormat.toLowerCase()){
      case "json":
        await writeFileWithTemplate(secrets, "templates/json.ejs", outputFile);
        break;
      case "cfg":
        await writeFileWithTemplate(secrets, "templates/cfg.ejs", outputFile);
        break;
      case "cfg-strict":
        await writeFileWithTemplate(secrets, "templates/cfg-strict.ejs", outputFile);
        break;
      case "files":
        await Promise.all(Object.keys(secrets).map(secretName => writeFile(path.join(program.decryptedPath, secretName), stringifyIfJson(secrets[secretName]))));
        break;
      case "custom":
        if (templatePath == "")
        {
          throw new Error(`Missing template file, cannot write output`);
        }
        await writeFileWithTemplate(secrets, templatePath, outputFile);
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
