import { Col, Empty, Row } from 'antd';
import type { Widget } from '../types/widget';
import { WidgetItem } from './WidgetItem';

interface WidgetGridProps {
  widgets: Widget[];
}

export function WidgetGrid({ widgets }: WidgetGridProps) {
  if (widgets.length === 0) {
    return <Empty description="No widgets yet" />;
  }

  return (
    <Row gutter={[16, 16]}>
      {widgets.map((widget) => (
        <Col key={widget.id} span={8}>
          <WidgetItem widget={widget} />
        </Col>
      ))}
    </Row>
  );
}
