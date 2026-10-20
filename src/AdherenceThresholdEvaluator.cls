/** Flow-facing evaluator; computes breaches but leaves Task routing and DML to Flow. */
public with sharing class AdherenceThresholdEvaluator {
    public class Request {
        @InvocableVariable(required=true) public Id carePlanId;
    }
    public class Response {
        @InvocableVariable public String highestThreshold;
        @InvocableVariable public List<String> breachedThresholds;
        @InvocableVariable public Decimal adherence30;
        @InvocableVariable public Boolean consecutiveMisses;
    }
    @InvocableMethod(label='Evaluate adherence thresholds')
    public static List<Response> evaluate(List<Request> requests) {
        // TODO: Bulkify calculation and return ADH-60 > CONSEC-3 > ADH-80 severity.
        return new List<Response>();
    }
}
