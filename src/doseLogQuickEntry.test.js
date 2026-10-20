import { createElement } from 'lwc';
import DoseLogQuickEntry from 'c/doseLogQuickEntry';

describe('c-dose-log-quick-entry', () => {
    afterEach(() => { while (document.body.firstChild) document.body.removeChild(document.body.firstChild); });

    it('renders the required entry controls', () => {
        const element = createElement('c-dose-log-quick-entry', { is: DoseLogQuickEntry });
        document.body.appendChild(element);
        expect(element.shadowRoot.querySelector('lightning-combobox')).not.toBeNull();
    });

    // TODO: Mock Apex and cover default, 14-day boundary, Notes limit, focus, save, and error states.
});
