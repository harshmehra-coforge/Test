// LWC JS controller for missed dose entry
import { LightningElement, api, track } from 'lwc';

/**
 * doseLogQuickEntry LWC: Handles missed dose entry UI and calls Apex for backend logic.
 */
export default class DoseLogQuickEntry extends LightningElement {
    @api recordId;
    @track doseDateTime;
    @track medication;
    @track reason;
    @track notes;

    // TODO: Wire up form handlers, call Apex insert/edit, handle errors
}
