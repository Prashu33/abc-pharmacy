// api.js - Thin fetch wrappers for the ABC Pharmacy API

const API_BASE = '/api';

export async function getMedicines(search = '') {
  const url = `${API_BASE}/medicines` + (search ? `?search=${encodeURIComponent(search)}` : '');
  const res = await fetch(url);
  if (!res.ok) throw new Error('Failed to fetch medicines');
  return res.json();
}

export async function getMedicine(id) {
  const res = await fetch(`${API_BASE}/medicines/${id}`);
  if (!res.ok) throw new Error(`Failed to fetch medicine ${id}`);
  return res.json();
}

export async function addMedicine(dto) {
  const res = await fetch(`${API_BASE}/medicines`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(dto)
  });
  if (!res.ok) {
    const errData = await res.json().catch(() => ({}));
    throw new Error(errData.message || 'Failed to add medicine');
  }
  return res.json();
}

export async function recordSale(dto) {
  const res = await fetch(`${API_BASE}/sales`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(dto)
  });
  if (!res.ok) {
    const errData = await res.json().catch(() => ({}));
    throw new Error(errData.message || 'Failed to record sale');
  }
  return res.json();
}

export async function getSales() {
  const res = await fetch(`${API_BASE}/sales`);
  if (!res.ok) throw new Error('Failed to fetch sales history');
  return res.json();
}
