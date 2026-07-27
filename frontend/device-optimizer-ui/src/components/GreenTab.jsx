import { useEffect, useState } from 'react';
import {
  Button,
  Card,
  Spinner,
  Table,
  TableHeader,
  TableRow,
  TableHeaderCell,
  TableBody,
  TableCell,
} from '@fluentui/react-components';
import { getGreenReport, downloadGreenReportPdf } from '../api/deviceApi';

export default function GreenTab({ isAdmin }) {
  const [report, setReport] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    getGreenReport()
      .then(setReport)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, []);

  return (
    <div>
      <h2>The Green Report </h2>
      <p>
        Every year a laptop stays in service past the typical 3-year replacement cycle avoids the CO2e
        of manufacturing a brand new one.
      </p>

      {loading && <Spinner label="Loading green report..." />}
      {error && <p style={{ color: 'red' }}>Error: {error}</p>}

      {!loading && !error && report && (
        <>
          <div style={{ display: 'flex', gap: '12px', marginBottom: '16px' }}>
            <Card style={{ padding: '12px', flex: 1 }}>
              <p style={{ margin: 0, fontWeight: 600 }}>CO2 avoided {isAdmin ? 'fleet-wide' : 'by your company'}</p>
              <p style={{ margin: '4px 0', fontSize: '24px' }}>{report.fleetAvoidedCo2Kg} kg</p>
            </Card>
            <Card style={{ padding: '12px', flex: 1 }}>
              <p style={{ margin: 0, fontWeight: 600 }}>🌳 Equivalent to</p>
              <p style={{ margin: '4px 0', fontSize: '24px' }}>{report.fleetTreesEquivalent} trees/year</p>
            </Card>
            <Card style={{ padding: '12px', flex: 1 }}>
              <p style={{ margin: 0, fontWeight: 600 }}>🚗 Equivalent to</p>
              <p style={{ margin: '4px 0', fontSize: '24px' }}>{report.fleetCarKmEquivalent} car km</p>
            </Card>
          </div>

          <Button appearance="primary" onClick={downloadGreenReportPdf}>
            Download PDF report
          </Button>

          <h3 style={{ marginTop: '24px' }}>{isAdmin ? 'Per-customer breakdown' : 'Your company'}</h3>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHeaderCell>Tenant</TableHeaderCell>
                <TableHeaderCell>Devices</TableHeaderCell>
                <TableHeaderCell>CO2 Avoided (kg)</TableHeaderCell>
                <TableHeaderCell>Trees / year</TableHeaderCell>
                <TableHeaderCell>Car km</TableHeaderCell>
              </TableRow>
            </TableHeader>
            <TableBody>
              {report.tenants.map((tenant) => (
                <TableRow key={tenant.tenantName}>
                  <TableCell>{tenant.tenantName}</TableCell>
                  <TableCell>{tenant.deviceCount}</TableCell>
                  <TableCell>{tenant.avoidedCo2Kg}</TableCell>
                  <TableCell>{tenant.treesEquivalent}</TableCell>
                  <TableCell>{tenant.carKmEquivalent}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>

          <p style={{ marginTop: '16px', fontSize: '13px', fontStyle: 'italic' }}>
            These figures are estimates based on published manufacturer environmental reports for typical
            laptop manufacturing emissions, not measurements of these exact devices.
          </p>
        </>
      )}
    </div>
  );
}
