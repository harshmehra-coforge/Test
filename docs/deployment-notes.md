# Deployment Notes

1. Map Meera and Rhea through environment-specific `Strategic_Approval_Config__mdt` records.
2. Deploy fields and permissions before Flows and Approval Processes.
3. Validate existing Deal Desk process names and handoff action in each org.
4. Configure Slack Named Credential with OAuth-managed secrets; do not store tokens in metadata.
5. Enable feature toggle only after sandbox boundary, bulk, and resilience tests pass.
6. Rollback by disabling the feature toggle, activating the prior Flow, and stopping the Scheduled Flow. Do not delete approval history.
