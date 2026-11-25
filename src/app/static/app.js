"""Browser orchestration; server responses remain authoritative for rating and validation."""
const $ = (id) => document.getElementById(id);
async function call(path, method, body) {
  const response = await fetch(path, {method, headers: {'Content-Type': 'application/json'}, body: JSON.stringify(body)});
  const data = await response.json();
  if (!response.ok) throw data.error || {message: 'Request failed'};
  return data;
}
$('quote-form').addEventListener('submit', async (event) => { event.preventDefault(); try { const data = await call('/api/v1/quotes', 'POST', {driver_age: Number($('age').value), vehicle_type: $('vehicle').value}); $('quote-result').textContent = JSON.stringify(data, null, 2); } catch (error) { $('message').textContent = error.message; } });
$('bind-form').addEventListener('submit', async (event) => { event.preventDefault(); try { const data = await call('/api/v1/policies', 'POST', {driver_age: Number($('age').value), vehicle_type: $('vehicle').value, effective_date: $('effective').value, expiration_date: $('expiration').value}); $('policy-result').textContent = JSON.stringify(data, null, 2); $('policy-id').value = data.policy_id; } catch (error) { $('message').textContent = error.message; } });
$('claim-form').addEventListener('submit', async (event) => { event.preventDefault(); try { const data = await call('/api/v1/claims', 'POST', {policy_id: $('policy-id').value, loss_date: $('loss-date').value, loss_type: $('loss-type').value, description: $('description').value}); $('claim-result').textContent = JSON.stringify(data, null, 2); $('claim-id').value = data.claim_id; } catch (error) { $('message').textContent = error.message; } });
$('status-form').addEventListener('submit', async (event) => { event.preventDefault(); try { const data = await call(`/api/v1/claims/${encodeURIComponent($('claim-id').value)}`, 'GET'); $('status-result').textContent = JSON.stringify(data, null, 2); } catch (error) { $('message').textContent = error.message; } });
