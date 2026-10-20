import { LightningElement, api } from 'lwc';
import loadContext from '@salesforce/apex/DoseLogService.loadContext';
import createDoseLog from '@salesforce/apex/DoseLogService.createDoseLog';

/** Care Plan quick-entry component for structured missed-dose records. */
export default class DoseLogQuickEntry extends LightningElement {
    @api recordId;
    medicationLabel = '';
    doseDateTime = new Date().toISOString();
    reason = '';
    notes = '';
    errorMessage = '';
    isLoading = false;
    isSaving = false;

    reasonOptions = [
        'Forgot', 'Side effects', 'Cost / access', 'Feeling better',
        'Travel', 'Hospitalization', 'Other'
    ].map((label) => ({ label, value: label }));

    connectedCallback() {
        this.load();
    }

    async load() {
        this.isLoading = true;
        try {
            const context = await loadContext({ carePlanId: this.recordId });
            this.medicationLabel = context.medicationLabel;
        } catch (error) {
            this.errorMessage = 'Unable to load missed-dose context.';
        } finally {
            this.isLoading = false;
        }
    }

    handleDateChange(event) { this.doseDateTime = event.detail.value; }
    handleReasonChange(event) { this.reason = event.detail.value; }
    handleNotesChange(event) { this.notes = event.detail.value; }
    handleCancel() { this.dispatchEvent(new CustomEvent('cancel')); }

    async handleSave() {
        // TODO: Add client-side 14-day validation and focus invalid fields.
        if (!this.reason || this.notes.length > 500) {
            this.errorMessage = 'Select a reason and keep notes within 500 characters.';
            return;
        }
        this.isSaving = true;
        try {
            await createDoseLog({ request: {
                carePlanId: this.recordId,
                doseDateTime: this.doseDateTime,
                reason: this.reason,
                notes: this.notes,
                idempotencyKey: crypto.randomUUID()
            }});
            this.dispatchEvent(new CustomEvent('saved'));
        } catch (error) {
            this.errorMessage = 'The missed dose could not be saved.';
        } finally {
            this.isSaving = false;
        }
    }
}
