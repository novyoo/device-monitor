import { useEffect, useState } from 'react';
import { FluentProvider, webLightTheme, TabList, Tab, Button, Spinner, Card } from '@fluentui/react-components';
import Dashboard from './components/Dashboard';
import ReturnsTab from './components/ReturnsTab';
import DoctorTab from './components/DoctorTab';
import GreenTab from './components/GreenTab';
import LoginPage from './components/LoginPage';
import { getCurrentUser, logout } from './api/deviceApi';

function PrivacyPanel() {
  return (
    <Card style={{ padding: '12px', marginBottom: '20px', fontSize: '13px' }}>
      <p style={{ margin: 0, fontWeight: 600 }}>What we collect / what we never collect</p>
      <p style={{ margin: '4px 0' }}>
        Collected: hardware vitals only — battery health, disk wear, disk errors, crash count, sudden
        shutdowns, temperature, RAM usage %, active-use hours, and days since the last OS update.
      </p>
      <p style={{ margin: 0 }}>
        Never collected: file names or contents, browsing history, installed apps, keystrokes,
        screenshots, location, or anything that identifies the person using the device.
      </p>
    </Card>
  );
}

export default function App() {
  const [currentUser, setCurrentUser] = useState(undefined);
  const [selectedTab, setSelectedTab] = useState('fleet');

  useEffect(() => {
    getCurrentUser().then(setCurrentUser);
  }, []);

  async function handleLogout() {
    await logout();
    setCurrentUser(null);
  }

  if (currentUser === undefined) {
    return (
      <FluentProvider theme={webLightTheme}>
        <div style={{ padding: '80px', textAlign: 'center' }}>
          <Spinner label="Loading..." />
        </div>
      </FluentProvider>
    );
  }

  if (!currentUser) {
    return (
      <FluentProvider theme={webLightTheme}>
        <LoginPage onLoginSuccess={setCurrentUser} />
      </FluentProvider>
    );
  }

  const isAdmin = currentUser.role === 'Admin';

  return (
    <FluentProvider theme={webLightTheme}>
      <div style={{ padding: '24px', maxWidth: '1300px', margin: '0 auto' }}>
        <div style={{ display: 'flex', flexWrap: 'wrap', justifyContent: 'space-between', alignItems: 'center', gap: '12px' }}>
          <h1>PULSLE</h1>
          <div style={{ textAlign: 'right' }}>
            <p style={{ margin: 0 }}>
              {currentUser.email} — {currentUser.role}
              {currentUser.tenantName ? ` (${currentUser.tenantName})` : ''}
            </p>
            <Button size="small" onClick={handleLogout}>Log out</Button>
          </div>
        </div>

        <PrivacyPanel />

        <TabList selectedValue={selectedTab} onTabSelect={(_, data) => setSelectedTab(data.value)}>
          <Tab value="fleet">Fleet</Tab>
          <Tab value="doctor">Doctor</Tab>
          <Tab value="returns">Returns Inbox</Tab>
          <Tab value="green">Green Report</Tab>
        </TabList>

        <div style={{ marginTop: '20px' }}>
          {selectedTab === 'fleet' && <Dashboard isAdmin={isAdmin} />}
          {selectedTab === 'doctor' && <DoctorTab />}
          {selectedTab === 'returns' && <ReturnsTab isAdmin={isAdmin} />}
          {selectedTab === 'green' && <GreenTab isAdmin={isAdmin} />}
        </div>
      </div>
    </FluentProvider>
  );
}
