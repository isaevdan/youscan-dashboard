import { Alert, Button, Skeleton, Space, Typography } from 'antd';
import { useCreateWidget, useWidgets } from '../api/useWidgets';
import { WidgetGrid } from './WidgetGrid';

export function Dashboard() {
  const widgetsQuery = useWidgets();
  const createMutation = useCreateWidget();

  return (
    <div style={{ padding: 24 }}>
      <Typography.Title level={2}>Dashboard</Typography.Title>
      <Space style={{ marginBottom: 24 }}>
        <Button onClick={() => createMutation.mutate('LineChart')}>Add Line Chart</Button>
        <Button onClick={() => createMutation.mutate('BarChart')}>Add Bar Chart</Button>
        <Button onClick={() => createMutation.mutate('Text')}>Add Text</Button>
      </Space>

      {widgetsQuery.isLoading && <Skeleton active />}
      {widgetsQuery.isError && (
        <Alert type="error" message="Failed to load widgets" description="Please try refreshing the page." />
      )}
      {widgetsQuery.isSuccess && <WidgetGrid widgets={widgetsQuery.data} />}
    </div>
  );
}
