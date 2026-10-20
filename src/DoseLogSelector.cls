/** Bounded selector for Dose_Log__c records; isolate org-specific field names here. */
public with sharing class DoseLogSelector {
    public static List<SObject> findRecentByCarePlan(Id carePlanId, Datetime startTime) {
        // TODO: Replace with confirmed Dose_Log__c fields and selective SOQL.
        return new List<SObject>();
    }
}
