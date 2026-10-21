// doseLogQuickEntry.js
// LWC controller for Missed Dose Quick Entry
import { LightningElement, api, track } from 'lwc';

/**
 * Missed Dose Quick Entry LWC
 * Handles UI logic, field prefill, validation, and submit
 */
export default class DoseLogQuickEntry extends LightningElement {
    @api recordId; // Care Plan record
    @track doseDate = new Date();
    @track medication = '';
    @track reason = '';
    @track notes = '';

    // TODO: Prefill medication from Care Plan
    // TODO: Enforce tab order, accessibility
    // TODO: Validate date (not older than 14 days)
    // TODO: Submit to Apex (DoseLogService.insertDoseLog)
    // TODO: Handle error/success
}
