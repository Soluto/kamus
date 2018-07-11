#!/usr/bin/env bash 

set -e

kubectl delete -f job.yaml
kubectl apply -f job.yaml

COUNTER=0

until [ $COUNTER -eq 4 ]; do
  SUCCESS=$(kubectl get job hamuste-smoke-test --namespace=team-devops -o json | jq '.status.succeeded')

  if [ -z "$SUCCESS" ] && [ $SUCCESS -eq 1 ]; then
    echo "job completed successfully"
    exit 0
  fi

  COUNTER=$((COUNTER + 1))

  echo $COUNTER

  sleep 5
done

echo "timeout while waiting for job to complete"
exit 1