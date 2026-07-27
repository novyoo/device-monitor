import { useEffect, useState } from 'react';
import {
  Button,
  Card,
  Field,
  Input,
  Dropdown,
  Option,
  Spinner,
  MessageBar,
  MessageBarBody,
} from '@fluentui/react-components';
import { getTenants, register, confirmEmail, login } from '../api/deviceApi';

function useConfirmEmailFromUrl() {
  const [state, setState] = useState({ checked: false, message: null, error: null });

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const userId = params.get('confirmUserId');
    const token = params.get('confirmToken');
    if (!userId || !token) {
      setState({ checked: true, message: null, error: null });
      return;
    }

    confirmEmail(userId, token)
      .then((data) => setState({ checked: true, message: data.message, error: null }))
      .catch((err) => setState({ checked: true, message: null, error: err.message }))
      .finally(() => {
        window.history.replaceState({}, '', window.location.pathname);
      });
  }, []);

  return state;
}

export default function LoginPage({ onLoginSuccess }) {
  const confirmState = useConfirmEmailFromUrl();

  const [mode, setMode] = useState('login');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [tenants, setTenants] = useState([]);
  const [tenantId, setTenantId] = useState(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState(null);
  const [infoMessage, setInfoMessage] = useState(null);

  useEffect(() => {
    if (mode === 'register' && tenants.length === 0) {
      getTenants().then(setTenants).catch(() => {});
    }
  }, [mode, tenants.length]);

  async function handleSubmit(event) {
    event.preventDefault();
    setError(null);
    setInfoMessage(null);
    setBusy(true);
    try {
      if (mode === 'login') {
        const user = await login(email, password);
        onLoginSuccess(user);
      } else {
        if (!tenantId) {
          setError('Please choose which company this account belongs to.');
          return;
        }
        const result = await register(email, password, tenantId);
        setInfoMessage(result.message);
        setMode('login');
        setPassword('');
      }
    } catch (err) {
      setError(err.message);
    } finally {
      setBusy(false);
    }
  }

  return (
    <div style={{ maxWidth: '420px', margin: '80px auto' }}>
      <h1 style={{ textAlign: 'center' }}>PULSLE</h1>

      {confirmState.message && (
        <MessageBar intent="success" style={{ marginBottom: '16px' }}>
          <MessageBarBody>{confirmState.message}</MessageBarBody>
        </MessageBar>
      )}
      {confirmState.error && (
        <MessageBar intent="error" style={{ marginBottom: '16px' }}>
          <MessageBarBody>{confirmState.error}</MessageBarBody>
        </MessageBar>
      )}

      <Card style={{ padding: '24px' }}>
        <h2 style={{ marginTop: 0 }}>{mode === 'login' ? 'Log in' : 'Create an account'}</h2>

        <form onSubmit={handleSubmit}>
          <Field label="Email" required style={{ marginBottom: '12px' }}>
            <Input value={email} onChange={(_, data) => setEmail(data.value)} type="email" />
          </Field>

          <Field label="Password" required style={{ marginBottom: '12px' }}>
            <Input value={password} onChange={(_, data) => setPassword(data.value)} type="password" />
          </Field>

          {mode === 'register' && (
            <Field label="Company" required style={{ marginBottom: '12px' }}>
              <Dropdown
                placeholder="Choose your company"
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
          )}

          {error && (
            <MessageBar intent="error" style={{ marginBottom: '12px' }}>
              <MessageBarBody>{error}</MessageBarBody>
            </MessageBar>
          )}
          {infoMessage && (
            <MessageBar intent="success" style={{ marginBottom: '12px' }}>
              <MessageBarBody>{infoMessage}</MessageBarBody>
            </MessageBar>
          )}

          <Button type="submit" appearance="primary" disabled={busy} style={{ width: '100%' }}>
            {busy ? <Spinner size="tiny" /> : mode === 'login' ? 'Log in' : 'Create account'}
          </Button>
        </form>

        <p style={{ textAlign: 'center', marginBottom: 0 }}>
          {mode === 'login' ? (
            <Button appearance="transparent" onClick={() => { setMode('register'); setError(null); }}>
              Need an account? Create one
            </Button>
          ) : (
            <Button appearance="transparent" onClick={() => { setMode('login'); setError(null); }}>
              Already have an account? Log in
            </Button>
          )}
        </p>
      </Card>

      <Card style={{ padding: '16px', marginTop: '16px', fontSize: '13px' }}>
        <p style={{ margin: 0, fontWeight: 600 }}>Demo accounts (password for all: FleetPulse-Yrl-2026!)</p>
        <p style={{ margin: '4px 0' }}>admin@fleetpulse.demo — YRL staff, sees every company's devices</p>
        <p style={{ margin: '4px 0' }}>sakura@fleetpulse.demo — Sakura Trading's own devices only</p>
        <p style={{ margin: 0 }}>fuji@ / tokyo@ / sunrise@fleetpulse.demo — the other three companies</p>
      </Card>
    </div>
  );
}
