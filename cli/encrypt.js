const keytar = require('keytar');
var bluebird = require('bluebird');
const opn = require('opn');
const { AuthenticationContext } = require('adal-node');
const activeDirectoryEndpoint = "https://login.microsoftonline.com/";
const fetch = require("node-fetch");

module.exports = async (args, options, logger) => {
    if (useAuth(options)) {
        const token = await acquireToken(options);
        await encrypt(args, options, token);
    }
    else {
        await encrypt(args, options)
    }
}

const encrypt = async ({ data, serviceAccount, namespace }, { kamusUrl }, token = null) => {
    console.log('Encryption started...')
    console.log('service account:', serviceAccount)
    console.log('namespace:', namespace)

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

        console.log(`Successfully encrypted data to ${serviceAccount} service account in ${namespace} namespace`);
        console.log('Encrypted data:\n' + await response.text());
        process.exit(0);
    }
    catch (err)
    {
        console.error('Error while trying to encrypt with kamus:', err.message);
        process.exit(1);
    }
}

const handleEncryptionError = (response) => {
    console.error('Error while trying to encrypt with kamus');
    if (response.status == 400) {
        console.error('Server returned bad request, make sure the service account and namespace exists');
    }
    if (response.status == 403) {
        console.error('Server returned authentication error, make sure your user has access rights to kamus');
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
    console.log(`Enter this code to authenticate: ${userCodeResult.userCode} Be quick you have 20 seconds`);
    opn(userCodeResult.verificationUrl);
    const deviceCodeResult =
        await context.acquireTokenWithDeviceCodeAsync(authResource, authApplication, userCodeResult);
    return deviceCodeResult.refreshToken;
};

const useAuth = ({ authTenant, authApplication, authResource }) => {
    if (authTenant && authApplication && authResource) {
        return true;
    }
    else {
        console.warn('Auth options were not provided, will trying to encrypt without authentication to kamus');
        return false;
    }
}