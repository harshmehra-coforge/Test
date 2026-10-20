/** Adapter for the approved PHI_Access_Log__c write mechanism. */
public with sharing class PhiAuditWriter {
    public static void writeDoseLogEvent(Id patientId, String action) {
        // TODO: Delegate to the existing helper or Platform Event pattern after confirmation.
        if (patientId == null || String.isBlank(action)) {
            throw new AuditException('Audit context is incomplete.');
        }
    }
    public class AuditException extends Exception {}
}
