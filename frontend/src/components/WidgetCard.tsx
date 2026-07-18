import type { ReactNode } from 'react';
import { DeleteOutlined } from '@ant-design/icons';
import { Alert, Button, Card, Popconfirm } from 'antd';

interface WidgetCardProps {
  title: string;
  onDelete: () => void;
  isDeleting: boolean;
  deleteError?: string;
  children: ReactNode;
}

export function WidgetCard({ title, onDelete, isDeleting, deleteError, children }: WidgetCardProps) {
  return (
    <Card
      title={title}
      loading={isDeleting}
      extra={
        <Popconfirm title="Delete this widget?" okText="Yes" cancelText="No" onConfirm={onDelete}>
          <Button type="text" danger icon={<DeleteOutlined />} aria-label="delete" />
        </Popconfirm>
      }
    >
      {deleteError && <Alert type="error" message={deleteError} style={{ marginBottom: 12 }} />}
      {children}
    </Card>
  );
}
