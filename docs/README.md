# Strategic Account Opportunity Approval Enhancement

Salesforce DX scaffold for CRO approval governance of Strategic Account Opportunities.

## Prerequisites

- Salesforce CLI (`sf`)
- A Salesforce sandbox or scratch org with Sales Cloud, Flow, Approval Processes, and Slack integration enabled
- Environment-specific Salesforce User IDs for Meera and Rhea

## Deploy

```bash
sf org login web --alias strategic-approval
sf project deploy start --target-org strategic-approval --source-dir force-app
sf apex run test --target-org strategic-approval --test-level RunLocalTests --wait 30
```

Review `PLAN.md` before activation. The Flow, approval process, Slack connection, and report metadata contain administrator binding TODOs because names and IDs are org-specific.

## Key rules

- CRO: Strategic Account snapshot and `TCV__c > 100000`.
- Deal Desk predecessor: `Discount_Percent__c > 10`.
- Rhea escalation: only `TCV__c < 500000` after 72 elapsed hours.
- Idempotency key: Opportunity ID plus submission version.

## Development

```bash
sf project deploy start --target-org strategic-approval --source-dir force-app
sf apex run test --target-org strategic-approval --tests StrategicApprovalRouterTest --target-org strategic-approval
```

Do not enable production Flow versions until UAT, security review, threshold boundary tests, notification failure tests, and rollback rehearsal are complete.
