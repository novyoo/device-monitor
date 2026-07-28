import { useEffect, useState } from 'react';
import {
  Button,
  Dialog,
  DialogSurface,
  DialogBody,
  DialogTitle,
  DialogContent,
  DialogActions,
  Dropdown,
  Option,
  Field,
  Input,
} from '@fluentui/react-components';
import { getTenants, registerDevice } from '../api/deviceApi';

export default function RegisterDeviceDialog({ open, onClose, onRegistered }) {
  const [tenants, setTenants] = useState([]);
  const [model, setModel] = useState('');
  const [serialNumber, setSerialNumber] = useState('');
  const [tenantId, setTenantId] = useState(null);
  const [purchaseDate, setPurchaseDate] = useState('');
  const [result, setResult] = useState(null);
  const [error, setError] = useState(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (open && tenants.length === 0) {
      getTenants().then(setTenants).catch(() => {});
    }
  }, [open, tenants.length]);

  function reset() {
    setModel('');
    setSerialNumber('');
    setTenantId(null);
    setPurchaseDate('');
    setResult(null);
    setError(null);
  }

  function handleClose() {
    reset();
    onClose();
    if (result) onRegistered();
  }

  async function handleRegister() {
    setError(null);
    if (!model || !serialNumber || !tenantId) {
      setError('Model, serial number and tenant are all required.');
      return;
    }
    setSaving(true);
    try {
      const registered = await registerDevice(model, serialNumber, tenantId, purchaseDate || null);
      setResult(registered);
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  }

  return (
    <Dialog open={open} onOpenChange={(_, data) => { if (!data.open) handleClose(); }}>
      <DialogSurface>
        <DialogBody>
          <DialogTitle>Register a real device</DialogTitle>
          <DialogContent>
            {!result && (
              <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
                <Field label="Model">
                  <Input value={model} onChange={(_, data) => setModel(data.value)} placeholder="e.g. HP Pavilion 15" />
                </Field>
                <Field label="Serial number">
                  <Input value={serialNumber} onChange={(_, data) => setSerialNumber(data.value)} placeholder="e.g. REAL-000001" />
                </Field>
                <Field label="Tenant">
                  <Dropdown
                    placeholder="Choose a tenant"
                    value={tenants.find((t) => t.id === tenantId)?.name ?? ''}
                    onOptionSelect={(_, data) => setTenantId(Number(data.optionValue))}
                  >
                    {tenants.map((t) => (
                      <Option key={t.id} value={String(t.id)}>
                        {t.name}
                      </Option>
                    ))}
                  </Dropdown>
                </Field>
                <Field label="Purchase date (optional)">
                  <Input type="date" value={purchaseDate} onChange={(_, data) => setPurchaseDate(data.value)} />
                </Field>
                {error && <p style={{ color: 'red' }}>{error}</p>}
              </div>
            )}
            {result && (
              <div>
                <p>Device registered. Give this API key to the device's owner — the agent needs it to check in.</p>
                <p style={{ fontFamily: 'monospace', fontSize: 16, wordBreak: 'break-all' }}>{result.apiKey}</p>
                <p>Save it now: it is only shown here once.</p>
              </div>
            )}
          </DialogContent>
          <DialogActions>
            {!result && (
              <Button appearance="primary" onClick={handleRegister} disabled={saving}>
                Register
              </Button>
            )}
            <Button onClick={handleClose}>{result ? 'Done' : 'Cancel'}</Button>
          </DialogActions>
        </DialogBody>
      </DialogSurface>
    </Dialog>
  );
}
