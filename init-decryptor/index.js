let program = require('commander');
const readfiles = require('node-readfiles');
const fs = require('fs');
const util = require('util');
const readFile = util.promisify(fs.readFile);
const writeFile = util.promisify(fs.writeFile);
const axios = require('axios');

program
    .version('0.1.0')
    .option('-e, --encrypted-folder [path]', 'Encrypted files folder path')
    .option('-d, --decrypted-file [path]', 'Decrypted JSON path')
    .parse(process.argv);

let getEncryptedFiles = async () => {
  return await readfiles(program.encryptedFolder, function (err, filename, contents) {
    if (err) throw err;
    console.log('File ' + filename + ':');
    console.log(contents);
  });
}

let getkamusUrl = () => {
    let url = process.env.KAMUS_URL;
    if (!url) {
        throw new Error("Missing KAMUS_URL env var");
    }
    return url;
}

let getBarerToken = async () => {
    return await readFile("/var/run/secrets/kubernetes.io/serviceaccount/token");
}

let decryptFile = async (httpClient, filePath) => {
    var encryptedContent = await readFile(filePath);
    const response = await httpClient.post('/api/v1/decrypt', encryptedContent);

}

async function run() {
    let files = await getEncryptedFiles();
    let kamusUrl = getkamusUrl();
    let token = await getBarerToken();

    const httpClient = axios.create({
        baseURL: kamusUrl,
        timeout: 1000,
        headers: {"Content-Type": "application/json", "Authorization": "Bearer " + token}
    });

    let secrets = {};

    files.forEach(file => {
        secrets[file] = decryptFile(httpClient, file);
    });

    await writeFile(program.decryptedFile, JSON.parse(secrets));
    
    console.log("Decrypted: " + secrets.keys())
}

run()