const expect = require('chai').expect;
const nock = require('nock');
const sinon = require('sinon');
const fs = require('fs');
const mockFS = require('mock-fs');

const encrypt = require('../actions/encrypt');

const logger =
{
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

let kamusApiScope;

describe('Encrypt', () => {

  beforeEach(() => {
    sinon.stub(process, 'exit');
    kamusApiScope = nock(kamusUrl)
      .post('/api/v1/encrypt', { data: secret, ['service-account']: serviceAccount, namespace })
      .reply(200, encryptedSecret);
  });

  afterEach(() => {
    process.exit.restore();
  });

  it('Should return encrypted data', async () => {
    await encrypt(null, { secret, serviceAccount, namespace, kamusUrl }, logger);
    expect(kamusApiScope.isDone()).to.be.true;
    expect(process.exit.called).to.be.true;
    expect(process.exit.calledWith(0)).to.be.true;
    expect(logger.info.lastCall.lastArg).to.equal(`Encrypted data:\n${encryptedSecret}`);
  });

  describe('Input from file', () => {

    const path = 'path/to/file';
    const existingFile = 'existing.txt';
    const unexistingFile = 'not-found.txt';
    before(() => {
      mockFS({
        [path]: {
          [existingFile]: secret,
        },
      });
    });

    after(() => {
      mockFS.restore();
    });

    it('should encrypt data from file', async () => {
      const file = `${path}/${existingFile}`;
      await encrypt(null, { file, serviceAccount, namespace, kamusUrl}, logger);
      expect(kamusApiScope.isDone()).to.be.true;
      expect(process.exit.called).to.be.true;
      expect(process.exit.calledWith(0)).to.be.true;
      expect(logger.info.lastCall.lastArg).to.equal(`Encrypted data:\n${encryptedSecret}`);
    });

    it('should fail if neither secret or secret file options were set', async () => {
      const file = `${path}/${existingFile}`;
      await encrypt(null, { serviceAccount, namespace, kamusUrl }, logger);
      expect(kamusApiScope.isDone()).to.be.false;
      expect(process.exit.called).to.be.true;
      expect(process.exit.calledWith(1)).to.be.true;
    });

    it('should fail if both secret and secret file options were set', async () => {
      const file = `${path}/${existingFile}`;
      await encrypt(null, { secret, file, serviceAccount, namespace, kamusUrl }, logger);
      expect(kamusApiScope.isDone()).to.be.false;
      expect(process.exit.called).to.be.true;
      expect(process.exit.calledWith(1)).to.be.true;
    });

    it('should fail if the file doesn\'t exists', async () => {
      const file = `${path}/${unexistingFile}`;
      await encrypt(null, { file, serviceAccount, namespace, kamusUrl }, logger);
      expect(kamusApiScope.isDone()).to.be.false;
      expect(process.exit.called).to.be.true;
      expect(process.exit.calledWith(1)).to.be.true;
    });

  });

  describe('Save to file', () => {
    const path = 'path/to/outputDir';
    const newOutputFile = 'secret1.txt';
    const existingFile = 'secret2.txt';
    const existingFileContent = 'some content here';

    before(() => {
      mockFS({
        [path]: {
          [existingFile]: existingFileContent,
        }
      });
    });

    after(() => {
      mockFS.restore();
    });

    it('should save if the file doesn\'t exist', async () => {
      const outputFile = `${path}/${newOutputFile}`;
      await encrypt(null, { secret, serviceAccount, namespace, kamusUrl, outputFile}, logger);
      expect(kamusApiScope.isDone()).to.be.true;
      expect(process.exit.called).to.be.true;
      expect(process.exit.calledWith(0)).to.be.true;
      expect(fs.readFileSync(outputFile, { encoding: 'utf8' })).to.equal(encryptedData);
      expect(logger.info.lastCall.lastArg).to.equal(`Encrypted data was saved to ${outputFile}.`);
    });

    it('should fail if the file does exist', async () => {
      const outputFile = `${path}/${existingFile}`;
      await encrypt(null, { secret, serviceAccount, namespace, kamusUrl, outputFile}, logger);
      expect(kamusApiScope.isDone()).to.be.true;
      expect(process.exit.called).to.be.true;
      expect(process.exit.calledWith(1)).to.be.true;
      expect(fs.readFileSync(outputFile, { encoding: 'utf8' })).to.equal(existingFileContent);
    });

    it('should save if the file exists but overwrite flag added', async () => {
      const outputFile = `${path}/${existingFile}`;
      await encrypt(null, { secret, serviceAccount, namespace, kamusUrl, outputFile, overwrite: true}, logger);
      expect(kamusApiScope.isDone()).to.be.true;
      expect(process.exit.calledWith(0)).to.be.true;
      expect(fs.readFileSync(outputFile, { encoding: 'utf8' })).to.equal(encryptedData);
      expect(logger.info.lastCall.lastArg).to.equal(`Encrypted data was saved to ${outputFile}.`);
    });

  });
});