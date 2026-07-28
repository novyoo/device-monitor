const API_BASE = import.meta.env.DEV ? 'https://localhost:7080/api' : '/api';

async function getJson(path) {
  const response = await fetch(`${API_BASE}${path}`, { credentials: 'include' });
  if (!response.ok) {
    throw new Error(`Request failed: ${response.status}`);
  }
  return response.json();
}

export function getDevices() {
  return getJson('/devices');
}

export function getReturnedDevices() {
  return getJson('/devices/returns');
}

export function getReturnStats() {
  return getJson('/devices/returns/stats');
}

export function getDeviceDetail(id) {
  return getJson(`/devices/${id}/detail`);
}

export function getGreenReport() {
  return getJson('/green/report');
}

export function downloadGreenReportPdf() {
  window.location.href = `${API_BASE}/green/report/pdf`;
}

async function postAction(path) {
  const response = await fetch(`${API_BASE}${path}`, { method: 'POST', credentials: 'include' });
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

export function resellDevice(id) {
  return postAction(`/devices/${id}/resell`);
}

export function retireDevice(id) {
  return postAction(`/devices/${id}/retire`);
}

async function postJson(path, body) {
  const response = await fetch(`${API_BASE}${path}`, {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });
  const data = await response.json().catch(() => null);
  if (!response.ok) {
    const message = Array.isArray(data) ? data.join(' ') : data?.message ?? data ?? `Request failed: ${response.status}`;
    throw new Error(message);
  }
  return data;
}

export function registerDevice(model, serialNumber, tenantId, purchaseDate) {
  return postJson('/devices/register', { model, serialNumber, tenantId, purchaseDate });
}

export function getTenants() {
  return getJson('/auth/tenants');
}

export function register(email, password, tenantId) {
  return postJson('/auth/register', { email, password, tenantId });
}

export function confirmEmail(userId, token) {
  return postJson('/auth/confirm-email', { userId, token });
}

export function login(email, password) {
  return postJson('/auth/login', { email, password });
}

export async function logout() {
  await fetch(`${API_BASE}/auth/logout`, { method: 'POST', credentials: 'include' });
}

export async function getCurrentUser() {
  try {
    const response = await fetch(`${API_BASE}/auth/me`, { credentials: 'include' });
    if (!response.ok) return null;
    return await response.json();
  } catch {
    return null;
  }
}
