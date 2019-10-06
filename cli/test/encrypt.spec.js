/* eslint-disable security/detect-non-literal-fs-filename */
const expect = require('chai').expect;
const nock = require('nock');
const sinon = require('sinon');
const fs = require('fs');
const mockFS = require('mock-fs');

const encrypt = require('../lib/actions/encrypt');

const logger =
{
    debug: sinon.spy(),
    info: sinon.spy(),
    error: sinon.spy(),
    warn: sinon.spy(),
    log: sinon.spy(),
};

const kamusUrl = 'https://kamus.com';
const secret = 'super-secret';
const encryptedSecret = '123ABC';
const serviceAccount = 'dummy';
const namespace = 'team-a';

describe('Encrypt', () => {

  let kamusApiScope;
  let kamusApiWithPrefix;
  const inputPath = 'path/to/inputFile';
  const outputPath = 'path/to/outputDir';
  const newOutputFile = 'new.txt';
  const existingFile = 'existing.txt';
  const nonexistingFile = 'not-found.txt';
  const existingFileContent = 'some content here';

  before(() => {
    mockFS({
      [outputPath]: {
        [existingFile]: secret,
        [existingFile]: existingFileContent,
      },
      [inputPath]: {
        [existingFile]: secret,
      }
    });
  });

  after(() => {
    mockFS.restore();
  });

  beforeEach(() => {
    sinon.stub(process, 'exit');
    kamusApiScope = nock(kamusUrl)
      .post('/api/v1/encrypt', { data: secret, ['service-account']: serviceAccount, namespace })
      .reply(200, encryptedSecret);

    kamusApiWithPrefix = nock(kamusUrl)
      .post('/prefix/api/v1/encrypt', { data: secret, ['service-account']: serviceAccount, namespace })
      .reply(200, encryptedSecret);
  });

  afterEach(() => {
    nock.cleanAll()
    process.exit.restore();
  });
  

  it('Should return encrypted data', async () => {
    await encrypt(null, { secret, serviceAccount, namespace, kamusUrl }, logger);
    expect(kamusApiScope.isDone()).to.be.true;
    expect(process.exit.called).to.be.true;
    expect(process.exit.calledWith(0)).to.be.true;
    expect(logger.info.lastCall.lastArg).to.equal(`Encrypted data:\n${encryptedSecret}`);
  });
  
  it('Should return encrypted data with / in the end of the url', async () => {
    let kamusUrlWithSlash = kamusUrl + '/';
    await encrypt(null, { secret, serviceAccount, namespace, kamusUrl: kamusUrlWithSlash }, logger);
    expect(kamusApiScope.isDone()).to.be.true;
    expect(process.exit.called).to.be.true;
    expect(process.exit.calledWith(0)).to.be.true;
    expect(logger.info.lastCall.lastArg).to.equal(`Encrypted data:\n${encryptedSecret}`);
  });

  it('should support path in kamus url', async () => {
    let url = 'https://kamus.com/prefix';
    await encrypt(null, { secret, serviceAccount, namespace, kamusUrl: url }, logger);
    expect(kamusApiWithPrefix.isDone()).to.be.true;
    expect(process.exit.called).to.be.true;
    expect(process.exit.calledWith(0)).to.be.true;
    expect(logger.info.lastCall.lastArg).to.equal(`Encrypted data:\n${encryptedSecret}`);
  });

  describe('Save to file', () => {

    it('should save if the file doesn\'t exist', async () => {
      const outputFile = `${outputPath}/${newOutputFile}`;
      await encrypt(null, { secret, serviceAccount, namespace, kamusUrl, outputFile}, logger);
      expect(kamusApiScope.isDone()).to.be.true;
      expect(process.exit.called).to.be.true;
      expect(process.exit.calledWith(0)).to.be.true;
      expect(fs.readFileSync(outputFile, { encoding: 'utf8' })).to.equal(encryptedSecret);
      expect(logger.info.lastCall.lastArg).to.equal(`Encrypted data was saved to ${outputFile}.`);
    });

    it('should fail if the file does exist', async () => {
      const outputFile = `${outputPath}/${existingFile}`;
      await encrypt(null, { secret, serviceAccount, namespace, kamusUrl, outputFile}, logger);
      expect(kamusApiScope.isDone()).to.be.true;
      expect(process.exit.called).to.be.true;
      expect(process.exit.calledWith(1)).to.be.true;
      expect(fs.readFileSync(outputFile, { encoding: 'utf8' })).to.equal(existingFileContent);
    });

    it('should save if the file exists but overwrite flag added', async () => {
      const outputFile = `${outputPath}/${existingFile}`;
      await encrypt(null, { secret, serviceAccount, namespace, kamusUrl, outputFile, overwrite: true}, logger);
      expect(kamusApiScope.isDone()).to.be.true;
      expect(process.exit.calledWith(0)).to.be.true;
      expect(fs.readFileSync(outputFile, { encoding: 'utf8' })).to.equal(encryptedSecret);
      expect(logger.info.lastCall.lastArg).to.equal(`Encrypted data was saved to ${outputFile}.`);
    });

  });

  describe('Input from file', () => {

    it('should encrypt data from file', async () => {
      const secretFile = `${inputPath}/${existingFile}`;
      await encrypt(null, { secretFile, serviceAccount, namespace, kamusUrl }, logger);
      expect(kamusApiScope.isDone()).to.be.true;
      expect(process.exit.called).to.be.true;
      expect(process.exit.calledWith(0)).to.be.true;
      expect(logger.info.lastCall.lastArg).to.equal(`Encrypted data:\n${encryptedSecret}`);
    });

    it('should fail if neither secret or secret file options were set', async () => {
      await encrypt(null, { serviceAccount, namespace, kamusUrl }, logger);
      expect(kamusApiScope.isDone()).to.be.false;
      expect(process.exit.called).to.be.true;
      expect(process.exit.calledWith(1)).to.be.true;
    });

    it('should fail if both secret and secret file options were set', async () => {
      const secretFile = `${inputPath}/${existingFile}`;
      await encrypt(null, { secret, secretFile, serviceAccount, namespace, kamusUrl }, logger);
      expect(kamusApiScope.isDone()).to.be.false;
      expect(process.exit.called).to.be.true;
      expect(process.exit.calledWith(1)).to.be.true;
    });

    it('should fail if the file doesn\'t exists', async () => {
      const file = `${inputPath}/${nonexistingFile}`;
      await encrypt(null, { file, serviceAccount, namespace, kamusUrl }, logger);
      expect(kamusApiScope.isDone()).to.be.false;
      expect(process.exit.called).to.be.true;
      expect(process.exit.calledWith(1)).to.be.true;
    });

  });
});