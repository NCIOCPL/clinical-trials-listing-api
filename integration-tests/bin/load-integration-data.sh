#!/bin/bash

## Use the Environment var or the default
if [[ -z "${ELASTIC_SEARCH_HOST}" ]]; then
    ELASTIC_HOST="http://localhost:9200"
else
    ELASTIC_HOST=${ELASTIC_SEARCH_HOST}
fi

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

## Wait until docker is up.
echo "Waiting for ES Service at ${ELASTIC_HOST} to Start"
until $(curl --output /dev/null --silent --head --fail "${ELASTIC_HOST}"); do
    printf '.'
    sleep 1
done
echo "ES Service is up"

# First wait for ES to start...
response=$(curl --write-out %{http_code} --silent --output /dev/null "${ELASTIC_HOST}")

until [ "$response" = "200" ]; do
    response=$(curl --write-out %{http_code} --silent --output /dev/null "${ELASTIC_HOST}")
    >&2 echo "Elastic Search is unavailable - sleeping"
    sleep 1
done

# next wait for ES status to turn to Green
health_check="curl -fsSL ${ELASTIC_HOST}/_cat/health?h=status"
health=$(eval $health_check)
echo "Waiting for ES status to be ready"
until [[ "$health" = 'green' ]]; do
    >&2 echo "Elastic Search is unavailable - sleeping"
    sleep 10
    health=$(eval $health_check)
done
echo "ES status is green"

pushd $DIR/../data/source
echo "Load the index mapping"
MAPPING_LOAD_CMD="curl -fsS -H \"Content-Type: application/x-ndjson\" -XPUT ${ELASTIC_HOST}/drugv1 --data-binary \"@drug-es-mapping.json\""
mapping_output=$(eval $MAPPING_LOAD_CMD)
echo $mapping_output

echo "Load the records"
BULK_LOAD_CMD="curl -fsS -H \"Content-Type: application/x-ndjson\" -XPOST ${ELASTIC_HOST}/_bulk --data-binary \"@drug-data.jsonl\""
load_output=$(eval $BULK_LOAD_CMD)

## Test to make sure we loaded items
## TODO: Actually check for errors.
echo $curl_output | wc -l

popd
