/** Typed contracts for Strategic Account approval routing. */
public with sharing class StrategicApprovalModels {
    public class SubmissionRequest {
        @InvocableVariable(required=true) public Id opportunityId;
        @InvocableVariable public String idempotencyKey;
    }

    public class RoutingResult {
        public Boolean croRequired;
        public Boolean dealDeskRequired;
        public Integer submissionVersion;
        public String snapshotJson;
        public String idempotencyKey;
    }

    public class RoutingConfig {
        public Decimal strategicTcvMinimum;
        public Decimal dealDeskDiscountMinimum;
        public Integer croSlaHours;
        public Decimal chiefOfStaffApprovalCeiling;
        public Boolean featureEnabled;
        public String policyVersion;
    }

    public class SlaResult {
        public Boolean escalated;
        public Boolean actionableForChiefOfStaff;
        public String reason;
    }
}
