/** Flow entry points for controlled submission and Deal Desk handoff. */
public with sharing class StrategicApprovalInvocable {
    @InvocableMethod(label='Submit Strategic Approval' description='Snapshots routing and starts the governed approval chain.')
    public static List<StrategicApprovalModels.RoutingResult> submit(List<StrategicApprovalModels.SubmissionRequest> requests) {
        List<StrategicApprovalModels.RoutingResult> results = new List<StrategicApprovalModels.RoutingResult>();
        StrategicApprovalServices.SubmissionService service =
            new StrategicApprovalServices.SubmissionService(new StrategicApprovalRepository());
        for (StrategicApprovalModels.SubmissionRequest request : requests) {
            // TODO: Add bulk-safe orchestration and surface actionable Flow faults.
            results.add(service.submit(request));
        }
        return results;
    }

    @InvocableMethod(label='Complete Deal Desk Handoff' description='Starts CRO approval after Deal Desk approval.')
    public static void handoff(List<StrategicApprovalModels.SubmissionRequest> requests) {
        StrategicApprovalServices.DealDeskHandoffService service = new StrategicApprovalServices.DealDeskHandoffService();
        for (StrategicApprovalModels.SubmissionRequest request : requests) {
            // TODO: Obtain the persisted submission version and perform guarded handoff.
            service.handoff(request.opportunityId, 0);
        }
    }
}
