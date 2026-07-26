const API_BASE = 'http://localhost:5080/api';

export async function getDevices() {
  const response = await fetch(`${API_BASE}/devices`);
  if (!response.ok) {
    throw new Error(`Failed to load devices: ${response.status}`);
  }
  return response.json();
}

export async function getReturnStats() {
  const response = await fetch(`${API_BASE}/devices/returns/stats`);
  if (!response.ok) {
    throw new Error(`Failed to load return stats: ${response.status}`);
  }
  return response.json();
}

async function postAction(path) {
  const response = await fetch(`${API_BASE}${path}`, { method: 'POST' });
  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || `Request failed: ${response.status}`);
  }
}

export function rentDevice(id) {
  return postAction(`/devices/${id}/rent`);
}

export function returnDevice(id) {
  return postAction(`/devices/${id}/return`);
}

export function restockDevice(id) {
  return postAction(`/devices/${id}/restock`);
}

export function repairDevice(id) {
  return postAction(`/devices/${id}/repair`);
}

export function retireDevice(id) {
  return postAction(`/devices/${id}/retire`);
}
