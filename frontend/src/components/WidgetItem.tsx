import { useDeleteWidget, useUpdateWidgetText } from '../api/useWidgets';
import type { Widget } from '../types/widget';
import { BarChartWidget } from './BarChartWidget';
import { LineChartWidget } from './LineChartWidget';
import { TextWidget } from './TextWidget';
import { WidgetCard } from './WidgetCard';

interface WidgetItemProps {
  widget: Widget;
}

export function WidgetItem({ widget }: WidgetItemProps) {
  const deleteMutation = useDeleteWidget();
  const updateMutation = useUpdateWidgetText();

  return (
    <WidgetCard
      title={widget.type}
      onDelete={() => deleteMutation.mutate(widget.id)}
      isDeleting={deleteMutation.isPending}
      deleteError={deleteMutation.isError ? 'Failed to delete widget' : undefined}
    >
      {widget.type === 'Text' && (
        <TextWidget
          text={widget.data.text}
          onSave={(text) => updateMutation.mutate({ id: widget.id, text })}
          isSaving={updateMutation.isPending}
          saveError={updateMutation.isError ? 'Failed to save text' : undefined}
        />
      )}
      {widget.type === 'LineChart' && <LineChartWidget data={widget.data} />}
      {widget.type === 'BarChart' && <BarChartWidget data={widget.data} />}
    </WidgetCard>
  );
}
