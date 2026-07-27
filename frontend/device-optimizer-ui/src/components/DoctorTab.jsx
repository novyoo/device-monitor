import { useEffect, useState } from 'react';
import { Card, Spinner } from '@fluentui/react-components';
import { getDevices } from '../api/deviceApi';
import DeviceTable from './DeviceTable';

export default function DoctorTab() {
  const [devices, setDevices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  function loadDevices() {
    getDevices()
      .then(setDevices)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }

  useEffect(() => {
    loadDevices();
    const interval = setInterval(loadDevices, 10000);
    return () => clearInterval(interval);
  }, []);

  const healthyCount = devices.filter((d) => d.healthBand === 'Healthy').length;
  const watchCount = devices.filter((d) => d.healthBand === 'Watch').length;
  const actNowDevices = devices.filter((d) => d.healthBand === 'ActNow');
  const noDataCount = devices.filter((d) => !d.healthBand).length;

  return (
    <div>
      <h2>The Doctor</h2>
      <p>Fleet-wide health overview, based on each device's latest check-in.</p>

      {loading && <Spinner label="Loading fleet health..." />}
      {error && <p style={{ color: 'red' }}>Error: {error}</p>}

      {!loading && !error && (
        <>
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: '12px', marginBottom: '20px' }}>
            <Card style={{ padding: '12px', flex: 1 }}>
              <p style={{ margin: 0, fontWeight: 600 }}>🟢 Healthy</p>
              <p style={{ margin: '4px 0', fontSize: '24px' }}>{healthyCount}</p>
            </Card>
            <Card style={{ padding: '12px', flex: 1 }}>
              <p style={{ margin: 0, fontWeight: 600 }}>🟡 Watch</p>
              <p style={{ margin: '4px 0', fontSize: '24px' }}>{watchCount}</p>
            </Card>
            <Card style={{ padding: '12px', flex: 1 }}>
              <p style={{ margin: 0, fontWeight: 600 }}>🔴 Act now</p>
              <p style={{ margin: '4px 0', fontSize: '24px' }}>{actNowDevices.length}</p>
            </Card>
            <Card style={{ padding: '12px', flex: 1 }}>
              <p style={{ margin: 0, fontWeight: 600 }}>No data yet</p>
              <p style={{ margin: '4px 0', fontSize: '24px' }}>{noDataCount}</p>
            </Card>
          </div>

          <h3>Red list: devices that need attention now</h3>
          <DeviceTable devices={actNowDevices} emptyMessage="No devices are in the red right now." />
        </>
      )}
    </div>
  );
}
