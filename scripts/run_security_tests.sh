#!/usr/bin/env bash

set -e

docker run --rm -v $(pwd)/glue:/input soluto/glue-ci:1532426297485 sh -x /app/run_glue.sh /input/glue.json /input/report.json