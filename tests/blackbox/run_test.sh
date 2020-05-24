#!/bin/bash

# Abort script on error
set -e
set -x

function run_tests()
{
  dotnet test ./blackbox.csproj
}

if [ -z "$PROXY_URL" ]
then
  echo PROXY_URL is not set, not running security checks
  run_tests
else
  ZAP_URL=$(echo $PROXY_URL | sed -e 's/https\?:\/\///')
  ./wait-for-it.sh $ZAP_URL -t 300
  echo "ZAP is ready"

  curl -s --fail $PROXY_URL/JSON/core/action/newSession > /dev/null
  curl -s --fail $PROXY_URL/JSON/pscan/action/enableAllScanners > /dev/null
  curl -s --fail $PROXY_URL/JSON/core/action/clearExcludedFromProxy > /dev/null

  # Add the rules you wish to ignore on this line, after the ids query param.
  curl -s --fail $PROXY_URL/JSON/pscan/action/disableScanners/?ids=10049,10021 > /dev/null

  # Add the URLs you wish to ignore on this line, after the regex query param - regex supported.
  # curl -s --fail $PROXY_URL/JSON/core/action/excludeFromProxy/?regex=

  ./wait-for-it.sh $ENCRYPTOR -t 300
  ./wait-for-it.sh $DECRYPTOR -t 300

  run_tests

  echo "waiting for ZAP to finish scanning"

  while [ "$(curl --fail $PROXY_URL/JSON/pscan/view/recordsToScan 2> /dev/null | jq '.recordsToScan')" != '"0"' ]; do sleep 1; done

  if [ "$(curl --fail $PROXY_URL/JSON/core/view/urls/?zapapiformat=JSON\&formMethod=GET\&baseurl= 2> /dev/null | jq '.urls | length' > 0)" == '"0"' ]; 
  then 
    echo "No URL was accessed by ZAP"
    exit -55
  fi

  curl --fail $PROXY_URL/OTHER/core/other/jsonreport/?formMethod=GET --output /reports/report.json
  curl --fail $PROXY_URL/OTHER/core/other/htmlreport/?formMethod=GET --output /reports/report.html

  curl -s --fail $PROXY_URL/JSON/core/action/saveSession/?name=blackbox\&overwrite=true > /dev/null

  echo "ZAP scan completed"
fi
