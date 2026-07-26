import { useEffect, useState } from 'react';
import { Button, Spinner, Card } from '@fluentui/react-components';
import {
  getDevices,
  getReturnStats,
  restockDevice,
  repairDevice,
  retireDevice,
} from '../api/deviceApi';
import DeviceTable from './DeviceTable';

export default function ReturnsTab() {
  const [devices, setDevices] = useState([]);
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  function loadData() {
    setLoading(true);
    Promise.all([getDevices(), getReturnStats()])
      .then(([allDevices, returnStats]) => {
        setDevices(allDevices.filter((device) => device.status === 'Returned'));
        setStats(returnStats);
      })
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }

  useEffect(loadData, []);

  async function handleAction(action, id) {
    setError(null);
    try {
      await action(id);
      loadData();
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <div>
      <h2>Returns Inbox</h2>
      <p>Devices returned by customers, awaiting a decision.</p>

      {stats && (
        <Card style={{ padding: '12px', marginBottom: '16px', maxWidth: '420px' }}>
          <p style={{ margin: 0, fontWeight: 600 }}>{stats.month}</p>
          <p style={{ margin: '4px 0' }}>{stats.returnedThisMonth} devices returned this month</p>
          <p style={{ margin: '4px 0' }}>{stats.awaitingDecision} devices currently awaiting a decision</p>
        </Card>
      )}

      {loading && <Spinner label="Loading returns..." />}
      {error && <p style={{ color: 'red' }}>Error: {error}</p>}

      {!loading && !error && (
        <DeviceTable
          devices={devices}
          emptyMessage="No devices are currently awaiting a decision."
          renderActions={(device) => (
            <div style={{ display: 'flex', gap: '8px' }}>
              <Button size="small" onClick={() => handleAction(restockDevice, device.id)}>
                Rent Again
              </Button>
              <Button size="small" onClick={() => handleAction(repairDevice, device.id)}>
                Repair First
              </Button>
              <Button size="small" appearance="outline" onClick={() => handleAction(retireDevice, device.id)}>
                Retire
              </Button>
            </div>
          )}
        />
      )}
    </div>
  );
}
