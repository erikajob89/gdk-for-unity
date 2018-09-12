#!/usr/bin/env bash

set -e -u -o -x pipefail

cd "$(dirname "$0")/../"

ci/test.sh
ci/build-client.sh
ci/build-gamelogic.sh
ci/run-sonarqube.sh
