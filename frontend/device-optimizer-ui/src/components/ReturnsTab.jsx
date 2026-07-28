import { useEffect, useState } from 'react';
import { Button, Spinner, Card, Badge } from '@fluentui/react-components';
import {
  getReturnedDevices,
  getReturnStats,
  restockDevice,
  repairDevice,
  resellDevice,
  retireDevice,
} from '../api/deviceApi';
import { HealthBadge, RecommendationBadge } from './DeviceTable';
import HealthDetailDialog from './HealthDetailDialog';

function ReturnCard({ device, isAdmin, onAction, onShowHealth }) {
  function actionAppearance(action) {
    return device.recommendation === action ? 'primary' : 'outline';
  }

  return (
    <Card style={{ padding: '16px', flex: '1 1 380px' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: '8px' }}>
        <div>
          <p style={{ margin: 0, fontWeight: 600 }}>{device.model}</p>
          <p style={{ margin: '2px 0 0', fontSize: '13px', color: '#616161' }}>
            {device.serialNumber} · {device.tenantName} · {device.repairCount} repair(s) so far
          </p>
        </div>
        <Badge appearance="tint">{device.status}</Badge>
      </div>

      <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px', alignItems: 'center', margin: '12px 0' }}>
        <button
          type="button"
          onClick={() => onShowHealth(device.id)}
          style={{ background: 'none', border: 'none', padding: 0, cursor: 'pointer' }}
        >
          <HealthBadge score={device.healthScore} band={device.healthBand} />
        </button>
        <RecommendationBadge action={device.recommendation} reasons={device.recommendationReasons} />
      </div>

      {isAdmin && (
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px' }}>
          <Button size="small" appearance={actionAppearance('RentAgain')} onClick={() => onAction(restockDevice, device.id)}>
            Rent Again
          </Button>
          <Button size="small" appearance={actionAppearance('Repair')} onClick={() => onAction(repairDevice, device.id)}>
            Repair First
          </Button>
          <Button size="small" appearance={actionAppearance('Resale')} onClick={() => onAction(resellDevice, device.id)}>
            Resell
          </Button>
          <Button size="small" appearance={actionAppearance('Retire')} onClick={() => onAction(retireDevice, device.id)}>
            Retire
          </Button>
        </div>
      )}
    </Card>
  );
}

export default function ReturnsTab({ isAdmin }) {
  const [devices, setDevices] = useState([]);
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selectedDeviceId, setSelectedDeviceId] = useState(null);

  function loadData() {
    setLoading(true);
    Promise.all([getReturnedDevices(), getReturnStats()])
      .then(([returnedDevices, returnStats]) => {
        setDevices(returnedDevices);
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
      <p>
        Devices returned by customers, awaiting a decision.
        {isAdmin
          ? " The highlighted button is the system's recommendation — click it to accept, or click a different one to override."
          : ' YRL staff decide what happens next; this view shows their recommendation for each device.'}
      </p>

      {stats && (
        <Card style={{ padding: '12px', marginBottom: '16px', maxWidth: '420px' }}>
          <p style={{ margin: 0, fontWeight: 600 }}>{stats.month}</p>
          <p style={{ margin: '4px 0' }}>{stats.returnedThisMonth} devices returned this month</p>
          <p style={{ margin: '4px 0' }}>{stats.awaitingDecision} devices currently awaiting a decision</p>
          {stats.agreementPercent != null && (
            <p style={{ margin: 0 }}>
              Staff agreed with the system's recommendation {stats.agreementPercent}% of the time so far
            </p>
          )}
        </Card>
      )}

      {loading && <Spinner label="Loading returns..." />}
      {error && <p style={{ color: 'red' }}>Error: {error}</p>}

      {!loading && !error && devices.length === 0 && <p>No devices are currently awaiting a decision.</p>}

      {!loading && !error && devices.length > 0 && (
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: '16px' }}>
          {devices.map((device) => (
            <ReturnCard
              key={device.id}
              device={device}
              isAdmin={isAdmin}
              onAction={handleAction}
              onShowHealth={setSelectedDeviceId}
            />
          ))}
        </div>
      )}

      <HealthDetailDialog deviceId={selectedDeviceId} onClose={() => setSelectedDeviceId(null)} />
    </div>
  );
}
