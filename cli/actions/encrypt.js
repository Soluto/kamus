const bluebird = require('bluebird');
const opn = require('opn');
const { AuthenticationContext } = require('adal-node');
const activeDirectoryEndpoint = "https://login.microsoftonline.com/";
const isDocker = require('../is-docker');
const url = require('url')
const request = require('request');
const {promisify} = require('util');
var pjson = require('../package.json');

let _logger;

module.exports = async (args, options, logger) => {
    _logger = logger;
    if (useAuth(options)) {
        const token = await acquireToken(options);

        await encrypt(options, token);
    }
    else {
        await encrypt(options);
    }
}

const encrypt = async ({ data, serviceAccount, namespace, kamusUrl, allowInsecureUrl, certFingerprint, outputFile, override }, token = null) => {
    _logger.log('Encryption started...');
    _logger.log('service account:', serviceAccount);
    _logger.log('namespace:', namespace);

    if (!allowInsecureUrl && url.parse(kamusUrl).protocol !== 'https:') {
        _logger.error("Insecure Kamus URL is not allowed");
        process.exit(1);
    }
    try {
        const response = await performEncryptRequestAsync(data, serviceAccount, namespace, kamusUrl, certFingerprint, token);
        if (response && response.statusCode >= 300) {
            _logger.error(`Encrypt request failed due to unexpected error. Status code: ${response.statusCode}`);
            process.exit(1);
        }
        _logger.info(`Successfully encrypted data to ${serviceAccount} service account in ${namespace} namespace`);
        if (outputFile) {
            fs = require('fs');
            fs.writeFileSync(outputFile, response.body, {
                encoding: 'utf8',
                flag: override ? 'w' : 'wx',
            });
            _logger.info(`Encrypted data was saved to ${outputFile}.`);
        }
        else {
            _logger.info(`Encrypted data:\n${response.body}`);
        }
        process.exit(0);
    }
    catch (err) {
        _logger.error('Error while trying to encrypt with kamus:', err.message);
        process.exit(1);
    }
}

const acquireToken = async ({ authTenant, authApplication, authResource }) => {
    const context = new AuthenticationContext(`${activeDirectoryEndpoint}${authTenant}`);
    bluebird.promisifyAll(context);
    refreshToken = await acquireTokenWithDeviceCode(context, authApplication, authResource);
    const refreshTokenResponse =
        await context.acquireTokenWithRefreshTokenAsync(refreshToken, authApplication, null, authResource);
    return refreshTokenResponse.accessToken;
};

const acquireTokenWithDeviceCode = async (context, authApplication, authResource) => {
    const userCodeResult = await context.acquireUserCodeAsync(authResource, authApplication, 'en');
    await outputUserCodeInstructions(userCodeResult);
    const deviceCodeResult =
        await context.acquireTokenWithDeviceCodeAsync(authResource, authApplication, userCodeResult);
    return deviceCodeResult.refreshToken;
};

const outputUserCodeInstructions = async (userCodeResult) => {
    if (isDocker()) {
        _logger.info(`Login to https://microsoft.com/devicelogin Enter this code to authenticate: ${userCodeResult.userCode}`)
    } else {
        opn(userCodeResult.verificationUrl);
        _logger.info(`Enter this code to authenticate: ${userCodeResult.userCode}`);
    }
}

const useAuth = ({ authTenant, authApplication, authResource }) => {
    if (authTenant && authApplication && authResource) {
        return true;
    }
    _logger.warn('Auth options were not provided, will try to encrypt without authentication to kamus');
    return false;
}

//Source: http://hassansin.github.io/certificate-pinning-in-nodejs
const performEncryptRequest = (data, serviceAccount, namespace, kamusUrl, certficateFingerprint, token, cb) => {

    const headers = {
        'User-Agent': `kamus-cli-${pjson.version}`,
        'Content-Type': 'application/json'
    };

    if (token != null) {
        headers['Authorization'] = `Bearer ${token}`
    }

    const options = {
        url: kamusUrl + '/api/v1/encrypt',
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
            if(certficateFingerprint !== undefined && certficateFingerprint !== fingerprint) {
            // Abort request, optionally emit an error event
                req.emit('error', new Error(`Server fingerprint ${fingerprint} does not match provided fingerprint ${certficateFingerprint}`));
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

performEncryptRequestAsync = promisify(performEncryptRequest);