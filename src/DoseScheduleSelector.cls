/** Selects active Care Plan schedule data for bounded adherence calculations. */
public with sharing class DoseScheduleSelector {
    public static List<SObject> findOccurrences(Id carePlanId, Datetime startTime, Datetime endTime) {
        // TODO: Adapt recurrence expansion to the confirmed Dose_Schedule__c schema.
        return new List<SObject>();
    }
}
