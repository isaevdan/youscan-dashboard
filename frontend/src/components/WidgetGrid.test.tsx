import type { ReactElement } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import type { Widget } from '../types/widget';
import { WidgetGrid } from './WidgetGrid';

function renderWithClient(ui: ReactElement) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });
  return render(<QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>);
}

const widgets: Widget[] = [
  { id: 1, type: 'Text', row: 0, column: 0, data: { text: 'one' } },
  { id: 2, type: 'Text', row: 0, column: 1, data: { text: 'two' } },
  { id: 3, type: 'Text', row: 0, column: 2, data: { text: 'three' } },
  { id: 4, type: 'Text', row: 1, column: 0, data: { text: 'four' } },
];

describe('WidgetGrid', () => {
  it('renders a WidgetItem for each widget', () => {
    renderWithClient(<WidgetGrid widgets={widgets} />);

    expect(screen.getByText('one')).toBeInTheDocument();
    expect(screen.getByText('two')).toBeInTheDocument();
    expect(screen.getByText('three')).toBeInTheDocument();
    expect(screen.getByText('four')).toBeInTheDocument();
  });

  it('lays out widgets 3 per row using a 24-column grid (span 8)', () => {
    const { container } = renderWithClient(<WidgetGrid widgets={widgets} />);

    const cols = container.querySelectorAll('.ant-col-8');
    expect(cols).toHaveLength(4);
  });

  it('shows an empty state when there are no widgets', () => {
    renderWithClient(<WidgetGrid widgets={[]} />);

    expect(screen.getByText(/no widgets/i)).toBeInTheDocument();
  });
});
