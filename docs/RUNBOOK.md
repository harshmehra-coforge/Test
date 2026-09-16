# Operations Runbook

## Activation

1. Map Meera and Rhea in Custom Metadata for the target org.
2. Confirm Deal Desk process API name and existing approval behavior.
3. Activate only the validated Flow versions after UAT sign-off.
4. Verify Slack Named Credential and email templates.
5. Manually validate the first five production submissions.

## Incidents

- Sev-1: approval creation unavailable for 15 minutes; Salesforce admin on-call acknowledges within 15 minutes.
- Sev-2: duplicate approval, unauthorized flag edit, or notification failure above 5%; acknowledge within 30 minutes.
- Sev-3: single notification failure after retries or malformed snapshot.

## Rollback

Disable the feature toggle, activate the prior Flow version, and stop Scheduled Flow. Do not delete or alter existing approval records.
