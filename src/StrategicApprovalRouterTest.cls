/** Unit tests for strict boundaries and routing branches in the TSD. */
@IsTest
private class StrategicApprovalRouterTest {
    private static StrategicApprovalModels.Config config() {
        StrategicApprovalModels.Config c = new StrategicApprovalModels.Config();
        c.strategicTcvMinimum = 100000; c.dealDeskDiscountMinimum = 10;
        c.chiefOfStaffApprovalCeiling = 500000; c.croSlaHours = 72;
        c.reminderHours = 24; c.policyVersion = '1.0'; return c;
    }

    @IsTest static void exactTcvDoesNotRequireCro() {
        StrategicApprovalModels.RoutingInput i = new StrategicApprovalModels.RoutingInput();
        i.strategicAtSubmission = true; i.tcv = 100000; i.discountPercent = 0; i.submissionVersion = 1;
        StrategicApprovalModels.RoutingDecision d = StrategicApprovalRouter.decide(i, config(), UserInfo.getUserId());
        System.assertEquals(false, d.croRequired);
    }

    @IsTest static void directAndSequencedBranchesAreDistinct() {
        StrategicApprovalModels.RoutingInput i = new StrategicApprovalModels.RoutingInput();
        i.strategicAtSubmission = true; i.tcv = 100000.01; i.discountPercent = 10; i.submissionVersion = 1;
        System.assertEquals(true, StrategicApprovalRouter.decide(i, config(), UserInfo.getUserId()).directCro);
        i.discountPercent = 10.01;
        StrategicApprovalModels.RoutingDecision d = StrategicApprovalRouter.decide(i, config(), UserInfo.getUserId());
        System.assertEquals(true, d.croRequired); System.assertEquals(true, d.dealDeskRequired);
    }

    @IsTest static void authorityCeilingIsStrict() {
        System.assertEquals(true, StrategicApprovalRouter.chiefOfStaffMayApprove(499999.99, config()));
        System.assertEquals(false, StrategicApprovalRouter.chiefOfStaffMayApprove(500000, config()));
    }
}
