#!/usr/bin/env bash
# Deploy metadata after binding org-specific approval and notification references.
set -euo pipefail
TARGET_ORG="${1:?Usage: scripts/deploy.sh <target-org> }"
sf project deploy start --target-org "$TARGET_ORG" --source-dir force-app
# TODO: Run deployment-specific validation and activate only the approved Flow versions.
