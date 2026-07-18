import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import type { ChartData } from '../types/widget';

interface BarChartWidgetProps {
  data: ChartData;
}

export function BarChartWidget({ data }: BarChartWidgetProps) {
  return (
    <ResponsiveContainer width="100%" height={200}>
      <BarChart data={data.points}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="label" />
        <YAxis />
        <Tooltip />
        <Bar dataKey="value" fill="#1677ff" />
      </BarChart>
    </ResponsiveContainer>
  );
}
