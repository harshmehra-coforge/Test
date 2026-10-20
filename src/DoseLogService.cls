/** with-sharing application service for missed-dose context, persistence, adherence, and edits. */
public with sharing class DoseLogService {
    @AuraEnabled(cacheable=true)
    public static DoseLogDtos.ContextResult loadContext(Id carePlanId) {
        // TODO: Enforce read access and resolve active medication and current adherence.
        throw new ServiceException('Context loading is not implemented.');
    }

    @AuraEnabled
    public static DoseLogDtos.SaveResult createDoseLog(DoseLogDtos.CreateRequest request) {
        // TODO: Validate authorization, date range, Reason, Notes, idempotency, then insert and audit atomically.
        throw new ServiceException('Dose-log creation is not implemented.');
    }

    @AuraEnabled(cacheable=true)
    public static DoseLogDtos.AdherenceResult getAdherence(Id carePlanId) {
        return AdherenceCalculator.calculate(carePlanId);
    }

    @AuraEnabled
    public static DoseLogDtos.SaveResult updateDoseLog(Id doseLogId, String reason, String notes) {
        // TODO: Allow only original creator within 24 hours; preserve audit/change history.
        throw new ServiceException('Dose-log update is not implemented.');
    }

    public class ServiceException extends Exception {}
}
