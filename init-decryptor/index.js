let program = require('commander');
const readfiles = require('node-readfiles');
const fs = require('fs');
const util = require('util');
const readFileAsync = util.promisify(fs.readFile);
const writeFile = util.promisify(fs.writeFile);
const axios = require('axios');

program
    .version('0.1.0')
    .option('-e, --encrypted-folder [path]', 'Encrypted files folder path')
    .option('-d, --decrypted-file [path]', 'Decrypted JSON path')
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

async function run() {
    let files = await getEncryptedFiles();
    let kamusUrl = getKamusUrl();
    let token = await getBarerToken();

    const httpClient = axios.create({
        baseURL: kamusUrl,
        timeout: 1000,
        headers: {"Content-Type": "application/json", "Authorization": "Bearer " + token}
    });

    let secrets = {};

    for (let file of files)
    {
        secrets[file] = await decryptFile(httpClient, file);
    }
    
    await writeFile(program.decryptedFile, JSON.stringify(secrets));
    
    console.log("Decrypted: " + Object.keys(secrets))
}

run();