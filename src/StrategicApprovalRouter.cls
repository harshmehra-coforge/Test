/**
 * Pure routing policy for Strategic Account Opportunity submissions.
 * TODO: Wire this service into the chosen invocable Flow action if Apex invocation is approved.
 */
public with sharing class StrategicApprovalRouter {
    public static StrategicApprovalModels.RoutingDecision decide(
        StrategicApprovalModels.RoutingInput input,
        StrategicApprovalModels.Config config,
        Id opportunityId
    ) {
        if (input == null || config == null || opportunityId == null) {
            throw new StrategicApprovalException('Routing inputs are required.');
        }
        if (input.tcv == null || input.tcv < 0 || input.discountPercent == null ||
            input.discountPercent < 0 || input.discountPercent > 100) {
            throw new StrategicApprovalException('TCV and discount values are invalid.');
        }
        StrategicApprovalModels.RoutingDecision result = new StrategicApprovalModels.RoutingDecision();
        result.croRequired = input.strategicAtSubmission == true && input.tcv > config.strategicTcvMinimum;
        result.dealDeskRequired = input.discountPercent > config.dealDeskDiscountMinimum;
        result.directCro = result.croRequired && !result.dealDeskRequired;
        result.idempotencyKey = String.valueOf(opportunityId) + '+' + String.valueOf(input.submissionVersion);
        result.snapshotJson = JSON.serialize(new Map<String, Object>{
            'submissionVersion' => input.submissionVersion, 'accountId' => input.accountId,
            'strategicAtSubmission' => input.strategicAtSubmission, 'tcv' => input.tcv,
            'discountPercent' => input.discountPercent, 'strategicThreshold' => config.strategicTcvMinimum,
            'discountThreshold' => config.dealDeskDiscountMinimum, 'croRequired' => result.croRequired,
            'dealDeskRequired' => result.dealDeskRequired, 'routingPolicyVersion' => config.policyVersion
        });
        return result;
    }

    public static Boolean chiefOfStaffMayApprove(Decimal tcv, Config config) {
        return tcv != null && config != null && tcv < config.chiefOfStaffApprovalCeiling;
    }

    public class StrategicApprovalException extends Exception {}
}
