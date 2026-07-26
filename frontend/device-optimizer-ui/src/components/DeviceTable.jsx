import {
  Table,
  TableHeader,
  TableRow,
  TableHeaderCell,
  TableBody,
  TableCell,
  Badge,
} from '@fluentui/react-components';

function formatNumber(value, unit) {
  return value === null || value === undefined ? '—' : `${value}${unit}`;
}

function formatLastCheckIn(value) {
  return value ? new Date(value).toLocaleTimeString() : '—';
}

export default function DeviceTable({ devices, renderActions, emptyMessage }) {
  if (devices.length === 0) {
    return <p>{emptyMessage}</p>;
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHeaderCell>Tenant</TableHeaderCell>
          <TableHeaderCell>Model</TableHeaderCell>
          <TableHeaderCell>Serial Number</TableHeaderCell>
          <TableHeaderCell>Status</TableHeaderCell>
          <TableHeaderCell>Repairs</TableHeaderCell>
          <TableHeaderCell>Battery</TableHeaderCell>
          <TableHeaderCell>Disk Wear</TableHeaderCell>
          <TableHeaderCell>Crashes</TableHeaderCell>
          <TableHeaderCell>Temp</TableHeaderCell>
          <TableHeaderCell>Active Use</TableHeaderCell>
          <TableHeaderCell>Last Check-in</TableHeaderCell>
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
            <TableCell>{formatNumber(device.batteryHealthPercent, '%')}</TableCell>
            <TableCell>{formatNumber(device.diskWearPercent, '%')}</TableCell>
            <TableCell>{formatNumber(device.crashCount, '')}</TableCell>
            <TableCell>{formatNumber(device.temperatureCelsius, '°C')}</TableCell>
            <TableCell>{formatNumber(device.activeUseHours, 'h')}</TableCell>
            <TableCell>{formatLastCheckIn(device.lastCheckInAt)}</TableCell>
            {renderActions && <TableCell>{renderActions(device)}</TableCell>}
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
