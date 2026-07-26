import { useState } from 'react';
import { FluentProvider, webLightTheme, TabList, Tab } from '@fluentui/react-components';
import Dashboard from './components/Dashboard';
import ReturnsTab from './components/ReturnsTab';
import DoctorTab from './components/DoctorTab';

export default function App() {
  const [selectedTab, setSelectedTab] = useState('fleet');

  return (
    <FluentProvider theme={webLightTheme}>
      <div style={{ padding: '24px', maxWidth: '1000px', margin: '0 auto' }}>
        <h1>FleetPulse</h1>

        <TabList selectedValue={selectedTab} onTabSelect={(_, data) => setSelectedTab(data.value)}>
          <Tab value="fleet">Fleet</Tab>
          <Tab value="doctor">Doctor</Tab>
          <Tab value="returns">Returns Inbox</Tab>
        </TabList>

        <div style={{ marginTop: '20px' }}>
          {selectedTab === 'fleet' && <Dashboard />}
          {selectedTab === 'doctor' && <DoctorTab />}
          {selectedTab === 'returns' && <ReturnsTab />}
        </div>
      </div>
    </FluentProvider>
  );
}
