import { Alert, Button, Skeleton, Space, Typography } from 'antd';
import { useCreateWidget, useInfiniteWidgets } from '../api/useWidgets';
import { useInfiniteScrollSentinel } from '../hooks/useInfiniteScrollSentinel';
import { WidgetGrid } from './WidgetGrid';

export function Dashboard() {
  const widgetsQuery = useInfiniteWidgets();
  const createMutation = useCreateWidget();

  const sentinelRef = useInfiniteScrollSentinel(() => {
    if (!widgetsQuery.isFetchingNextPage) {
      widgetsQuery.fetchNextPage();
    }
  }, !!widgetsQuery.hasNextPage);

  const widgets = widgetsQuery.data?.pages.flatMap((page) => page.items) ?? [];

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
      {widgetsQuery.isSuccess && <WidgetGrid widgets={widgets} />}
      {widgetsQuery.hasNextPage && <div ref={sentinelRef} style={{ height: 1 }} />}
      {widgetsQuery.isFetchingNextPage && <Skeleton active style={{ marginTop: 16 }} />}
    </div>
  );
}
