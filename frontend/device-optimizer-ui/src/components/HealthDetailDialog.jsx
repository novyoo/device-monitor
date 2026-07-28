import { useEffect, useState } from 'react';
import {
  Dialog,
  DialogSurface,
  DialogBody,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Spinner,
  Badge,
} from '@fluentui/react-components';
import { getDeviceDetail } from '../api/deviceApi';

const bandColor = { Healthy: 'success', Watch: 'warning', ActNow: 'danger' };
const bandEmoji = { Healthy: '🟢', Watch: '🟡', ActNow: '🔴' };

function formatValue(value, unit = '') {
  return value === null || value === undefined ? '—' : `${value}${unit}`;
}

function barColor(score) {
  if (score >= 80) return '#0f7b0f';
  if (score >= 60) return '#c19c00';
  return '#c4314b';
}

function HealthHistoryBars({ history }) {
  if (history.length === 0) {
    return <p>Not enough history yet.</p>;
  }

  return (
    <div style={{ display: 'flex', alignItems: 'flex-end', gap: '3px', height: '80px' }}>
      {history.map((point, index) => (
        <div
          key={index}
          title={`${new Date(point.timestamp).toLocaleString()}: ${point.score}%`}
          style={{
            width: '8px',
            height: `${Math.max(point.score, 4)}%`,
            background: barColor(point.score),
          }}
        />
      ))}
    </div>
  );
}

export default function HealthDetailDialog({ deviceId, onClose }) {
  const [detail, setDetail] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (deviceId === null) {
      setDetail(null);
      return;
    }
    setLoading(true);
    setError(null);
    getDeviceDetail(deviceId)
      .then(setDetail)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, [deviceId]);

  return (
    <Dialog
      open={deviceId !== null}
      onOpenChange={(_, data) => {
        if (!data.open) onClose();
      }}
    >
      <DialogSurface>
        <DialogBody>
          <DialogTitle>{detail ? `${detail.model} — ${detail.serialNumber}` : 'Device health'}</DialogTitle>
          <DialogContent>
            {loading && <Spinner label="Loading health details..." />}
            {error && <p style={{ color: 'red' }}>Error: {error}</p>}

            {detail && (
              <div>
                <div style={{ marginBottom: '8px' }}>
                  {detail.healthScore === null || detail.healthScore === undefined ? (
                    <Badge appearance="tint">No data yet</Badge>
                  ) : (
                    <Badge appearance="tint" color={bandColor[detail.healthBand] ?? 'subtle'}>
                      {bandEmoji[detail.healthBand] ?? ''} {detail.healthScore}%
                    </Badge>
                  )}
                </div>

                <h4>The 9 hardware vitals</h4>
                <ul>
                  <li>Battery health: {formatValue(detail.batteryHealthPercent, '%')}</li>
                  <li>Disk wear: {formatValue(detail.diskWearPercent, '%')}</li>
                  <li>Disk read/write errors: {formatValue(detail.diskErrorCount)}</li>
                  <li>Crashes since last check-in: {formatValue(detail.crashCount)}</li>
                  <li>Sudden shutdowns since last check-in: {formatValue(detail.suddenShutdownCount)}</li>
                  <li>Temperature: {formatValue(detail.temperatureCelsius, '°C')}</li>
                  <li>RAM usage: {formatValue(detail.ramUsagePercent, '%')}</li>
                  <li>Active use hours: {formatValue(detail.activeUseHours, 'h')}</li>
                  <li>Days since last OS update: {formatValue(detail.daysSinceOsUpdate)}</li>
                </ul>

                {detail.reasons.length > 0 && (
                  <>
                    <h4>Why this score</h4>
                    <ul>
                      {detail.reasons.map((reason, i) => (
                        <li key={i}>{reason}</li>
                      ))}
                    </ul>
                  </>
                )}

                {detail.flags.length > 0 && (
                  <>
                    <h4>Flags</h4>
                    <ul>
                      {detail.flags.map((flag, i) => (
                        <li key={i}>{flag}</li>
                      ))}
                    </ul>
                  </>
                )}

                {detail.trendMessage && (
                  <>
                    <h4>Trend</h4>
                    <p>📉 {detail.trendMessage}</p>
                  </>
                )}

                <h4>Health history</h4>
                <HealthHistoryBars history={detail.history} />
              </div>
            )}
          </DialogContent>
          <DialogActions>
            <Button appearance="secondary" onClick={onClose}>
              Close
            </Button>
          </DialogActions>
        </DialogBody>
      </DialogSurface>
    </Dialog>
  );
}
