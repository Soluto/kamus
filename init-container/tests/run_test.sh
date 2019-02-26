#!/usr/bin/env bash

set -e

echo "Removing otput directory"

rm -rf output

echo "starting wiremock"

docker-compose up -d --build wiremock

docker-compose build decryptor

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

echo "comparing out.cfg and expected.cfg files"

diff -q output/out.cfg expected.cfg

rm -rf output

echo "running decryptor - files format"

OUTPUT_FORMAT=files docker-compose run decryptor

echo "comparing output directory with expected.json"

for f in "output"/*; do
  output=$(cat "$f")
  expected=$(cat expected.json | jq .$(basename $f) | sed -e 's/^"//' -e 's/"$//') # remove double qoutes because they doesn't exist in files
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