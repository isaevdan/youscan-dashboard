import type { ReactElement } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { server } from '../test/msw/server';
import { Dashboard } from './Dashboard';

const API_URL = 'http://localhost:8080';

function renderWithClient(ui: ReactElement) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });
  return render(<QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>);
}

describe('Dashboard', () => {
  it('starts empty and shows the empty state after loading', async () => {
    server.use(http.get(`${API_URL}/api/widgets`, () => HttpResponse.json([])));

    renderWithClient(<Dashboard />);

    expect(await screen.findByText(/no widgets/i)).toBeInTheDocument();
  });

  it('shows an error state when the initial fetch fails', async () => {
    server.use(
      http.get(`${API_URL}/api/widgets`, () => HttpResponse.json({ title: 'Server Error' }, { status: 500 })),
    );

    renderWithClient(<Dashboard />);

    expect(await screen.findByText(/failed to load/i)).toBeInTheDocument();
  });

  it('renders Add buttons for all three widget types', async () => {
    server.use(http.get(`${API_URL}/api/widgets`, () => HttpResponse.json([])));

    renderWithClient(<Dashboard />);
    await screen.findByText(/no widgets/i);

    expect(screen.getByRole('button', { name: /add line chart/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /add bar chart/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /add text/i })).toBeInTheDocument();
  });

  it('clicking Add Text creates a widget and it appears in the grid', async () => {
    let widgets: unknown[] = [];
    server.use(
      http.get(`${API_URL}/api/widgets`, () => HttpResponse.json(widgets)),
      http.post(`${API_URL}/api/widgets`, async ({ request }) => {
        const body = (await request.json()) as { type: string };
        const created = { id: 1, type: body.type, row: 0, column: 0, data: { text: '' } };
        widgets = [...widgets, created];
        return HttpResponse.json(created, { status: 201 });
      }),
    );

    const user = userEvent.setup();
    renderWithClient(<Dashboard />);

    await screen.findByText(/no widgets/i);
    await user.click(screen.getByRole('button', { name: /add text/i }));

    expect(await screen.findByRole('button', { name: /edit/i })).toBeInTheDocument();
  });
});
