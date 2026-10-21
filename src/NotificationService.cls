/**
 * NotificationService.cls
 * Handles sending notifications (email, Slack) for approval workflow events.
 */
public with sharing class NotificationService {
    /**
     * Sends notification to the specified user(s).
     * @param userIds List of User Ids
     * @param message Message to send
     * @param channel Notification channel (Email/Slack)
     */
    public static void sendNotification(List<Id> userIds, String message, String channel) {
        // TODO: Implement email/Slack notification logic
        throw new AuraHandledException('Not implemented');
    }

    /**
     * Notifies Finance on deep discount events (informational only).
     * @param opportunityId Id of the Opportunity
     */
    public static void notifyFinance(Id opportunityId) {
        // TODO: Send informational email to Finance
        throw new AuraHandledException('Not implemented');
    }
}
