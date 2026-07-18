export type WidgetType = 'LineChart' | 'BarChart' | 'Text';

export interface ChartPoint {
  label: string;
  value: number;
}

export interface ChartData {
  points: ChartPoint[];
}

export interface TextData {
  text: string;
}

interface BaseWidget {
  id: number;
  row: number;
  column: number;
}

export interface LineChartWidget extends BaseWidget {
  type: 'LineChart';
  data: ChartData;
}

export interface BarChartWidget extends BaseWidget {
  type: 'BarChart';
  data: ChartData;
}

export interface TextWidget extends BaseWidget {
  type: 'Text';
  data: TextData;
}

export type Widget = LineChartWidget | BarChartWidget | TextWidget;

export interface WidgetsPage {
  items: Widget[];
  hasMore: boolean;
  nextCursor: number | null;
}
