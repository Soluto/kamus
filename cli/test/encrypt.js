const expect = require('chai').expect;
const nock = require('nock');
const sinon = require('sinon');

const encrypt = require('../encrypt.js');

const logger = 
{
    info: sinon.spy(),
    error: console.error,
    warn: console.warn,
    log: console.log
};

const kamusUrl = 'https://kamus.com';
const data = 'super-secret';
const serviceAccount = 'dummy';
const namespace = 'team-a';

describe('Get User tests', () => {
  beforeEach(() => {
    sinon.stub(process, 'exit');
    nock(kamusUrl)
      .post('/api/v1/encrypt', { data, "service-account": serviceAccount, namespace})
      .reply(200, '123ABC');
  });

  it('Get a user by username', async () => {
    await encrypt({data, serviceAccount, namespace}, {kamusUrl}, logger);
    expect(process.exit.called).to.be.true;
    expect(process.exit.calledWith(0)).to.be.true;
    expect(logger.info.lastCall.lastArg).to.equal('Encrypted data:\n123ABC')
  });
});