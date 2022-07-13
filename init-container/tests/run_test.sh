#!/usr/bin/env bash

set -e

echo "Removing otput directory"

rm -rf output

echo "starting wiremock"

docker-compose up -d --build wiremock

docker-compose build decryptor

timeout 10s bash -c 'until docker-compose ps wiremock | grep \(healthy\); do sleep 1; done'

echo "running decryptor - json format"

OUTPUT_FORMAT=json docker-compose run decryptor

echo "comparing out.json and expected.json files"

diff -q output/out.json expected.json

rm -rf output

echo "running decryptor - cfg format"

OUTPUT_FORMAT=cfg docker-compose run decryptor

echo "comparing out.cfg and expected.cfg files"

diff -q output/out.cfg expected.cfg

rm -rf output

echo "running decryptor - cfg strict format"

OUTPUT_FORMAT=cfg-strict docker-compose run decryptor

echo "comparing out.cfg-strict and expected-strict.cfg files"

diff -q output/out.cfg-strict expected-strict.cfg

rm -rf output

echo "running decryptor - custom format"

cp templates/template.ejs encrypted/
OUTPUT_FORMAT=custom docker-compose run decryptor
rm -rf encrypted/template.ejs
echo "comparing out.custom and expected-custom.txt files"

diff -q output/out.custom expected-custom.txt

rm -rf output

echo "running decryptor - files format"

OUTPUT_FORMAT=files docker-compose run decryptor

echo "comparing output directory with expected.json"

for f in "output"/*; do
  output=$(cat "$f")
  if jq -e . >/dev/null 2>&1 <<<$output; then
    expected=$(cat expected.json | jq -cr .\"$(basename $f)\")
  else
    expected=$(cat expected.json | jq -r .$(basename $f))
  fi  
  diff <(echo $output) <(echo $expected)
done

rm -rf output

echo "running decryptor - upper case"

OUTPUT_FORMAT=JSON docker-compose run decryptor

if [[ $? != 0 ]]
then
  echo "should not fail on upper case format"
  exit 1
fi
