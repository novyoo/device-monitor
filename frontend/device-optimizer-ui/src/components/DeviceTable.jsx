import {
  Table,
  TableHeader,
  TableRow,
  TableHeaderCell,
  TableBody,
  TableCell,
  Badge,
} from '@fluentui/react-components';

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
            {renderActions && <TableCell>{renderActions(device)}</TableCell>}
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
