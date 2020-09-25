#!/bin/bash

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
$DIR/load-integration-data.sh

## Run Karate
$DIR/karate $DIR/../features
