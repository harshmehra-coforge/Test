/** Persistence boundary for Opportunity routing and operational events. */
public with sharing class StrategicApprovalRepository {
    public Opportunity loadOpportunity(Id opportunityId) {
        return [SELECT Id, AccountId, Account.Strategic_Account__c, TCV__c, Discount_Percent__c,
                Approval_Submission_Version__c, CRO_Approval_Required__c,
                Deal_Desk_Approval_Required__c, CRO_Escalated__c, CRO_Approval_Due__c
                FROM Opportunity WHERE Id = :opportunityId LIMIT 1];
    }

    public void saveRouting(Id opportunityId, StrategicApprovalModels.RoutingResult result) {
        // TODO: Persist snapshot and flags in a guarded, version-aware update.
    }

    public void logEvent(Id opportunityId, String eventType, String outcome, String correlationId) {
        // TODO: Insert Strategic_Approval_Execution_Log__c with retry/error metadata.
    }
}
