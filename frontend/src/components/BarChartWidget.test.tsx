import { render } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { BarChartWidget } from './BarChartWidget';

describe('BarChartWidget', () => {
  it('renders without crashing given chart data', () => {
    const { container } = render(
      <BarChartWidget
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
    const { container } = render(<BarChartWidget data={{ points: [] }} />);

    expect(container.querySelector('.recharts-responsive-container')).toBeInTheDocument();
  });
});
