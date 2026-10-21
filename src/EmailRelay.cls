/**
 * EmailRelay.cls
 * Handles email sending for notifications.
 */
public with sharing class EmailRelay {
    /**
     * Sends an email notification.
     * @param toAddress Recipient email address
     * @param subject Email subject
     * @param body Email body
     */
    public static void sendEmail(String toAddress, String subject, String body) {
        // TODO: Use Salesforce email service to send notification
        throw new AuraHandledException('Not implemented');
    }
}
