import { useState } from 'react';
import {
  Table,
  TableHeader,
  TableRow,
  TableHeaderCell,
  TableBody,
  TableCell,
  Badge,
} from '@fluentui/react-components';
import HealthDetailDialog from './HealthDetailDialog';

const bandColor = { Healthy: 'success', Watch: 'warning', ActNow: 'danger' };
const bandEmoji = { Healthy: '🟢', Watch: '🟡', ActNow: '🔴' };

function HealthBadge({ score, band }) {
  if (score === null || score === undefined) {
    return <Badge appearance="tint">No data yet</Badge>;
  }
  return (
    <Badge appearance="tint" color={bandColor[band] ?? 'subtle'}>
      {bandEmoji[band] ?? ''} {score}%
    </Badge>
  );
}

const recommendationLabel = { RentAgain: 'Rent Again', Repair: 'Repair First', Retire: 'Retire' };
const recommendationEmoji = { RentAgain: '✅', Repair: '🔧', Retire: '♻️' };
const recommendationColor = { RentAgain: 'success', Repair: 'warning', Retire: 'danger' };

function RecommendationBadge({ action, reasons }) {
  if (!action) {
    return <Badge appearance="tint">—</Badge>;
  }
  return (
    <Badge appearance="tint" color={recommendationColor[action] ?? 'subtle'} title={reasons?.join(' ')}>
      {recommendationEmoji[action] ?? ''} {recommendationLabel[action] ?? action}
    </Badge>
  );
}

export default function DeviceTable({ devices, renderActions, emptyMessage, showRecommendation }) {
  const [selectedDeviceId, setSelectedDeviceId] = useState(null);

  if (devices.length === 0) {
    return <p>{emptyMessage}</p>;
  }

  return (
    <>
      <Table>
        <TableHeader>
          <TableRow>
            <TableHeaderCell>Tenant</TableHeaderCell>
            <TableHeaderCell>Model</TableHeaderCell>
            <TableHeaderCell>Serial Number</TableHeaderCell>
            <TableHeaderCell>Status</TableHeaderCell>
            <TableHeaderCell>Repairs</TableHeaderCell>
            <TableHeaderCell>Health</TableHeaderCell>
            {showRecommendation && <TableHeaderCell>Recommendation</TableHeaderCell>}
            {renderActions && <TableHeaderCell>Actions</TableHeaderCell>}
          </TableRow>
        </TableHeader>
        <TableBody>
          {devices.map((device) => (
            <TableRow key={device.id}>
              <TableCell>{device.tenantName}</TableCell>
              <TableCell>{device.model}</TableCell>
              <TableCell>{device.serialNumber}</TableCell>
              <TableCell>
                <Badge appearance="tint">{device.status}</Badge>
              </TableCell>
              <TableCell>{device.repairCount}</TableCell>
              <TableCell>
                <button
                  type="button"
                  onClick={() => setSelectedDeviceId(device.id)}
                  style={{ background: 'none', border: 'none', padding: 0, cursor: 'pointer' }}
                >
                  <HealthBadge score={device.healthScore} band={device.healthBand} />
                </button>
              </TableCell>
              {showRecommendation && (
                <TableCell>
                  <RecommendationBadge action={device.recommendation} reasons={device.recommendationReasons} />
                </TableCell>
              )}
              {renderActions && <TableCell>{renderActions(device)}</TableCell>}
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <HealthDetailDialog deviceId={selectedDeviceId} onClose={() => setSelectedDeviceId(null)} />
    </>
  );
}
