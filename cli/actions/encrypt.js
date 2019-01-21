const bluebird = require('bluebird');
const opn = require('opn');
const url = require('url');
const fs = require('fs');
const request = require('request');
const { promisify } = require('util');
const { AuthenticationContext } = require('adal-node');
const activeDirectoryEndpoint = "https://login.microsoftonline.com/";

const  pjson = require('../package.json');

const isDocker = require('../is-docker');

const DEFAULT_ENCODING = 'utf8';

module.exports = async (args, options, logger) => {

    const { serviceAccount, namespace } = options;
    logger.log('Encryption started...');
    logger.log('service account:', serviceAccount);
    logger.log('namespace:', namespace);

    try {
        validateArguments(options);

        let token = null;
        if (!useAuth(options)) {
            logger.warn('Auth options were not provided, will try to encrypt without authentication to kamus');
        }
        else {
            token = await acquireToken(options, logger);
        }
        const encryptedSecret = await encrypt(options, token);

        logger.info(`Successfully encrypted data to ${serviceAccount} service account in ${namespace} namespace`);
        outputEncryptedSecret(encryptedSecret, options, logger);
        process.exit(0);
    }
    catch (err) {
        logger.error('Error while trying to encrypt with kamus:', err.message);
        process.exit(1);
    }
}

const encrypt = async ({ secret, file, serviceAccount, namespace, kamusUrl, certFingerprint, fileEncoding }, token = null) => {
    const data = file ? fs.readFileSync(file, { encoding: fileEncoding || DEFAULT_ENCODING }) : secret;
    const response = await performEncryptRequestAsync(data, serviceAccount, namespace, kamusUrl, certFingerprint, token);
    if (response && response.statusCode >= 300) {
        throw new Error(`Encrypt request failed due to unexpected error. Status code: ${response.statusCode}`);
    }
    return response.body;
};

const validateArguments = ({ secret, file, kamusUrl, allowInsecureUrl }) => {
    if (!secret && !file) {
        throw new Error('Neither secret nor secret-file options were set.');
    }

    if (secret && file) {
        throw new Error('Both secret nor secret-file options were set.');
    }

    if (!allowInsecureUrl && url.parse(kamusUrl).protocol !== 'https:') {
        throw new Error('Insecure Kamus URL is not allowed.');
    }
};

const acquireToken = async ({ authTenant, authApplication, authResource }, logger) => {
    const context = new AuthenticationContext(`${activeDirectoryEndpoint}${authTenant}`);
    bluebird.promisifyAll(context);
    const refreshToken = await acquireTokenWithDeviceCode(context, authApplication, authResource, logger);
    const refreshTokenResponse =
        await context.acquireTokenWithRefreshTokenAsync(refreshToken, authApplication, null, authResource);
    return refreshTokenResponse.accessToken;
};

const acquireTokenWithDeviceCode = async (context, authApplication, authResource, logger) => {
    const userCodeResult = await context.acquireUserCodeAsync(authResource, authApplication, 'en');
    await outputUserCodeInstructions(userCodeResult, logger);
    const deviceCodeResult =
        await context.acquireTokenWithDeviceCodeAsync(authResource, authApplication, userCodeResult);
    return deviceCodeResult.refreshToken;
};

const outputUserCodeInstructions = async (userCodeResult, logger) => {
    if (isDocker()) {
        logger.info(`Login to https://microsoft.com/devicelogin Enter this code to authenticate: ${userCodeResult.userCode}`)
    } else {
        opn(userCodeResult.verificationUrl);
        logger.info(`Enter this code to authenticate: ${userCodeResult.userCode}`);
    }
}

const useAuth = ({ authTenant, authApplication, authResource }, logger) => {
    if (authTenant && authApplication && authResource) {
        return true;
    }
    return false;
}

//Source: http://hassansin.github.io/certificate-pinning-in-nodejs
const performEncryptRequest = (data, serviceAccount, namespace, kamusUrl, certificateFingerprint, token, cb) => {
    const headers = {
        'User-Agent': `kamus-cli-${pjson.version}`,
        'Content-Type': 'application/json'
    };

    if (token != null) {
        headers['Authorization'] = `Bearer ${token}`
    }

    const options = {
        url: `${kamusUrl}/api/v1/encrypt`,
        headers: headers,
        // Certificate validation
        strictSSL: true,
        method: 'POST',
    };

    const req = request(options, cb);

    req.on('socket', socket => {
        socket.on('secureConnect', () => {
            const fingerprint = socket.getPeerCertificate().fingerprint;
            // Match the fingerprint with our saved fingerprints
            if(certificateFingerprint !== undefined && certificateFingerprint !== fingerprint) {
            // Abort request, optionally emit an error event
                req.emit('error', new Error(`Server fingerprint ${fingerprint} does not match provided fingerprint ${certificateFingerprint}`));
                return req.abort();
            }
        });
    });

    req.write(JSON.stringify({
        data,
        ['service-account']: serviceAccount,
        namespace,
    }));
}

const outputEncryptedSecret = (encryptedSecret, { outputFile, overwrite, fileEncoding }, logger) => {
    if (outputFile) {
        fs.writeFileSync(outputFile, encryptedSecret, {
            encoding: fileEncoding || DEFAULT_ENCODING,
            flag: overwrite ? 'w' : 'wx',
        });
        logger.info(`Encrypted data was saved to ${outputFile}.`);
    }
    else {
        logger.info(`Encrypted data:\n${encryptedSecret}`);
    }
};

performEncryptRequestAsync = promisify(performEncryptRequest);