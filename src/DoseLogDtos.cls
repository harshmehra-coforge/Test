/** Typed contracts shared by the LWC-facing service and calculation layers. */
public class DoseLogDtos {
    public class CreateRequest {
        @AuraEnabled public Id carePlanId;
        @AuraEnabled public Datetime doseDateTime;
        @AuraEnabled public String reason;
        @AuraEnabled public String notes;
        @AuraEnabled public String idempotencyKey;
    }
    public class AdherenceWindow {
        @AuraEnabled public Integer scheduledCount;
        @AuraEnabled public Integer missedCount;
        @AuraEnabled public Decimal percentage;
        @AuraEnabled public Boolean denominatorAvailable;
    }
    public class AdherenceResult {
        @AuraEnabled public AdherenceWindow days30;
        @AuraEnabled public AdherenceWindow days60;
        @AuraEnabled public AdherenceWindow days90;
        @AuraEnabled public Datetime calculatedAt;
        @AuraEnabled public List<String> warnings = new List<String>();
    }
    public class ContextResult {
        @AuraEnabled public Id carePlanId;
        @AuraEnabled public String medicationLabel;
        @AuraEnabled public AdherenceResult adherence;
        @AuraEnabled public Boolean canEdit;
    }
    public class SaveResult {
        @AuraEnabled public Id doseLogId;
        @AuraEnabled public AdherenceResult adherence;
        @AuraEnabled public Boolean auditWritten;
    }
}
