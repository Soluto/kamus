var bluebird = require('bluebird');
const opn = require('opn');
const { AuthenticationContext } = require('adal-node');
const activeDirectoryEndpoint = "https://login.microsoftonline.com/";
const fetch = require("node-fetch");
var url = require('url')
const isDocker = require('./is-docker');

let _logger;

module.exports = async (args, options, logger) => {
    _logger = logger;
    if (useAuth(options)) {
        const token = await acquireToken(options);
        await encrypt(args, options, token);
    }
    else {
        await encrypt(args, options)
    }
}

const encrypt = async ({ data, serviceAccount, namespace }, { kamusUrl, allowInsecureUrl }, token = null) => {
    _logger.log('Encryption started...');
    _logger.log('service account:', serviceAccount);
    _logger.log('namespace:', namespace);

    if (!allowInsecureUrl && url.parse(kamusUrl).protocol){
        _logger.error("Insecure Kamus URL is not allowed");
        process.exit(1);
    }

    try {
        var headers = { "Authorization": `Bearer ${token}`, "Content-Type": "application/json" };
        if (!token) delete headers["Authorization"];

        var response = await fetch(kamusUrl + '/api/v1/encrypt', {
            method: 'POST',
            body: JSON.stringify({
                data,
                "service-account": serviceAccount,
                namespace
            }),
            headers
        });

        if (!response.ok) handleEncryptionError(response);

        _logger.info(`Successfully encrypted data to ${serviceAccount} service account in ${namespace} namespace`);
        _logger.info('Encrypted data:\n' + await response.text());
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
        _logger.warn('Auth options were not provided, will trying to encrypt without authentication to kamus');
        return false;
    }
}