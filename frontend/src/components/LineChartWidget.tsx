import { CartesianGrid, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import type { ChartData } from '../types/widget';

interface LineChartWidgetProps {
  data: ChartData;
}

export function LineChartWidget({ data }: LineChartWidgetProps) {
  return (
    <ResponsiveContainer width="100%" height={200}>
      <LineChart data={data.points}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="label" />
        <YAxis />
        <Tooltip />
        <Line type="monotone" dataKey="value" stroke="#1677ff" />
      </LineChart>
    </ResponsiveContainer>
  );
}
