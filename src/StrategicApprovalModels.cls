/**
 * Domain contracts for Strategic Account approval routing.
 * Business orchestration remains in Salesforce Flow; this class is deterministic and testable.
 */
public with sharing class StrategicApprovalModels {
    public class RoutingInput {
        public Boolean strategicAtSubmission;
        public Decimal tcv;
        public Decimal discountPercent;
        public Id accountId;
        public Integer submissionVersion;
    }

    public class Config {
        public Decimal strategicTcvMinimum;
        public Decimal dealDeskDiscountMinimum;
        public Decimal chiefOfStaffApprovalCeiling;
        public Integer croSlaHours;
        public Integer reminderHours;
        public String policyVersion;
    }

    public class RoutingDecision {
        public Boolean croRequired;
        public Boolean dealDeskRequired;
        public Boolean directCro;
        public String idempotencyKey;
        public String snapshotJson;
    }
}
