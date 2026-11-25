"""Progressive browser client; server remains authoritative for all rules."""
const api = (path, options) => fetch(`/api/v1${path}`, {headers: {'Content-Type': 'application/json'}, ...options}).then(async r => { const body = await r.json(); if (!r.ok) throw body; return body; });
const formData = form => Object.fromEntries(new FormData(form));
const show = (id, value) => { document.getElementById(id).textContent = JSON.stringify(value, null, 2); };
document.getElementById('quote-form').onsubmit = async e => { e.preventDefault(); try { const body = formData(e.target); body.driver_age = Number(body.driver_age); show('quote-result', await api('/quotes', {method:'POST', body:JSON.stringify(body)})); } catch (e) { show('quote-result', e); } };
document.getElementById('policy-form').onsubmit = async e => { e.preventDefault(); try { show('policy-result', await api('/policies', {method:'POST', body:JSON.stringify(formData(e.target))})); } catch (e) { show('policy-result', e); } };
document.getElementById('claim-form').onsubmit = async e => { e.preventDefault(); try { show('claim-result', await api('/claims', {method:'POST', body:JSON.stringify(formData(e.target))})); } catch (e) { show('claim-result', e); } };
document.getElementById('status-form').onsubmit = async e => { e.preventDefault(); try { show('status-result', await api(`/claims/${formData(e.target).claim_id}`)); } catch (e) { show('status-result', e); } };
