var bluebird = require('bluebird');
const opn = require('opn');
const { AuthenticationContext } = require('adal-node');
const activeDirectoryEndpoint = "https://login.microsoftonline.com/";
const isDocker = require('./is-docker');
const url = require('url')
const request = require('request');
const {promisify} = require('util');

let _logger;

module.exports = async (args, options, logger) => {
    _logger = logger;
    console.log(`options: ${JSON.stringify(options)}`);
    if (useAuth(options)) {
        const token = await acquireToken(options);
        
        await encrypt(args, options, token);
    }
    else {
        await encrypt(args, options)
    }
}

const encrypt = async ({ data, serviceAccount, namespace }, { kamusUrl, allowInsecureUrl, certFingerprint }, token = null) => {
    _logger.log('Encryption started...');
    _logger.log('service account:', serviceAccount);
    _logger.log('namespace:', namespace);

    if (!allowInsecureUrl && url.parse(kamusUrl).protocol == 'http'){
        _logger.error("Insecure Kamus URL is not allowed");
        process.exit(1);
    }

    try {
        var response = await performEncryptRequestAsync(data, serviceAccount, namespace, kamusUrl, certFingerprint, token)
        if (response.statusCode >= 300) {
            _logger.error(`Encrypt request failed due to unexpected error. Status code: ${response.statusCode}`);
            process.exit(1);
        }
        _logger.info(`Successfully encrypted data to ${serviceAccount} service account in ${namespace} namespace`);
        _logger.info('Encrypted data:\n' + response.body);
        process.exit(0);
    }
    catch (err) {
        _logger.error('Error while trying to encrypt with kamus:', err.message);
        process.exit(1);
    }
}

const handleEncryptionError = (response) => {
    _logger.error('Error while trying to encrypt with kamus');
    if (response.status == 400) {
        _logger.error('Server returned bad request, make sure the service account and namespace exists');
    }
    if (response.status == 403) {
        _logger.error('Server returned authentication error, make sure your user has access rights to kamus');
    }
    process.exit(1);
}

const acquireToken = async ({ authTenant, authApplication, authResource }) => {
    const context = new AuthenticationContext(activeDirectoryEndpoint + authTenant);
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
    else {
        _logger.warn('Auth options were not provided, will try to encrypt without authentication to kamus');
        return false;
    }
}

//Source: http://hassansin.github.io/certificate-pinning-in-nodejs
const performEncryptRequest = (data, serviceAccount, namespace, kamusUrl, certficateFingerprint, token, cb) => {

    var headers = {
        'User-Agent': 'kamus-cli',
        'Content-Type': 'application/json'
    };

    if (token != null) {
        headers['Authorization'] = `Bearer ${token}`
    }

    var options = {
        url: kamusUrl + '/api/v1/encrypt',
        headers: headers,
        // Certificate validation
        strictSSL: true,
        method: 'POST',
    };
    
    var req = request(options, cb);
    
    req.on('socket', socket => {
        socket.on('secureConnect', () => {
            var fingerprint = socket.getPeerCertificate().fingerprint;
            // Match the fingerprint with our saved fingerprints
            if(certficateFingerprint != undefined && certficateFingerprint != fingerprint){
            // Abort request, optionally emit an error event
                req.emit('error', new Error(`Server fingerprint ${fingerprint} does not match provided fingerprint ${certficateFingerprint}`));
                return req.abort();
            }
        });
    });

    req.write(JSON.stringify({
        data,
        "service-account": serviceAccount,
        namespace
    }));
}

performEncryptRequestAsync = promisify(performEncryptRequest);