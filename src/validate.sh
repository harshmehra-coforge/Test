#!/usr/bin/env bash
set -euo pipefail
: "${SF_TARGET_ORG:?Set SF_TARGET_ORG to a Salesforce org alias}"
sf project deploy validate --manifest force-app/main/default/package.xml --target-org "$SF_TARGET_ORG"
sf apex run test --tests StrategicApprovalTests --target-org "$SF_TARGET_ORG" --result-format human
