import { render } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { LineChartWidget } from './LineChartWidget';

describe('LineChartWidget', () => {
  it('renders without crashing given chart data', () => {
    const { container } = render(
      <LineChartWidget
        data={{
          points: [
            { label: 'Mon', value: 10 },
            { label: 'Tue', value: 20 },
          ],
        }}
      />,
    );

    expect(container.querySelector('.recharts-responsive-container')).toBeInTheDocument();
  });

  it('renders without crashing given an empty points array', () => {
    const { container } = render(<LineChartWidget data={{ points: [] }} />);

    expect(container.querySelector('.recharts-responsive-container')).toBeInTheDocument();
  });
});
