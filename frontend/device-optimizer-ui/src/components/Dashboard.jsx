import { useEffect, useState } from 'react';
import { Button, Spinner } from '@fluentui/react-components';
import { getDevices, rentDevice, returnDevice } from '../api/deviceApi';
import DeviceTable from './DeviceTable';

export default function Dashboard() {
  const [devices, setDevices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  function loadDevices(showSpinner) {
    if (showSpinner) setLoading(true);
    getDevices()
      .then(setDevices)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }

  useEffect(() => {
    loadDevices(true);
    const interval = setInterval(() => loadDevices(false), 10000);
    return () => clearInterval(interval);
  }, []);

  async function handleAction(action, id) {
    setError(null);
    try {
      await action(id);
      loadDevices();
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <div>
      <p>{devices.length} devices loaded from the API</p>

      {loading && <Spinner label="Loading devices..." />}
      {error && <p style={{ color: 'red' }}>Error: {error}</p>}

      {!loading && !error && (
        <DeviceTable
          devices={devices}
          renderActions={(device) => {
            if (device.status === 'InStock') {
              return (
                <Button size="small" onClick={() => handleAction(rentDevice, device.id)}>
                  Rent Out
                </Button>
              );
            }
            if (device.status === 'Rented') {
              return (
                <Button size="small" onClick={() => handleAction(returnDevice, device.id)}>
                  Return
                </Button>
              );
            }
            return null;
          }}
        />
      )}
    </div>
  );
}
