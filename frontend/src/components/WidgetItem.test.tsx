import type { ReactElement } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { server } from '../test/msw/server';
import type { Widget } from '../types/widget';
import { WidgetItem } from './WidgetItem';

const API_URL = 'http://localhost:8080';

function renderWithClient(ui: ReactElement) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });
  return render(<QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>);
}

const textWidget: Widget = { id: 1, type: 'Text', row: 0, column: 0, data: { text: 'hello' } };
const chartWidget: Widget = {
  id: 2,
  type: 'LineChart',
  row: 0,
  column: 1,
  data: { points: [{ label: 'Mon', value: 5 }] },
};

describe('WidgetItem', () => {
  it('renders a Text widget with its text content', () => {
    renderWithClient(<WidgetItem widget={textWidget} />);
    expect(screen.getByText('hello')).toBeInTheDocument();
  });

  it('renders a LineChart widget', () => {
    const { container } = renderWithClient(<WidgetItem widget={chartWidget} />);
    expect(container.querySelector('.recharts-responsive-container')).toBeInTheDocument();
  });

  it('deleting a widget calls the DELETE endpoint', async () => {
    let called = false;
    server.use(
      http.delete(`${API_URL}/api/widgets/1`, () => {
        called = true;
        return new HttpResponse(null, { status: 204 });
      }),
    );
    const user = userEvent.setup();
    renderWithClient(<WidgetItem widget={textWidget} />);

    await user.click(screen.getByRole('button', { name: /delete/i }));
    await user.click(await screen.findByRole('button', { name: 'Yes' }));

    await waitFor(() => expect(called).toBe(true));
  });

  it('shows a delete error when the DELETE request fails', async () => {
    server.use(
      http.delete(`${API_URL}/api/widgets/1`, () =>
        HttpResponse.json({ title: 'Server Error' }, { status: 500 }),
      ),
    );
    const user = userEvent.setup();
    renderWithClient(<WidgetItem widget={textWidget} />);

    await user.click(screen.getByRole('button', { name: /delete/i }));
    await user.click(await screen.findByRole('button', { name: 'Yes' }));

    expect(await screen.findByText(/failed to delete/i)).toBeInTheDocument();
  });

  it('saving text on a Text widget calls the PUT endpoint with the new text', async () => {
    let receivedBody: unknown;
    server.use(
      http.put(`${API_URL}/api/widgets/1`, async ({ request }) => {
        receivedBody = await request.json();
        return HttpResponse.json({ ...textWidget, data: { text: 'updated' } });
      }),
    );
    const user = userEvent.setup();
    renderWithClient(<WidgetItem widget={textWidget} />);

    await user.click(screen.getByRole('button', { name: /edit/i }));
    await user.clear(screen.getByRole('textbox'));
    await user.type(screen.getByRole('textbox'), 'updated');
    await user.click(screen.getByRole('button', { name: /save/i }));

    await waitFor(() => expect(receivedBody).toEqual({ text: 'updated' }));
  });

  it('saving successfully exits edit mode', async () => {
    server.use(
      http.put(`${API_URL}/api/widgets/1`, () =>
        HttpResponse.json({ ...textWidget, data: { text: 'updated' } }),
      ),
    );
    const user = userEvent.setup();
    renderWithClient(<WidgetItem widget={textWidget} />);

    await user.click(screen.getByRole('button', { name: /edit/i }));
    await user.click(screen.getByRole('button', { name: /save/i }));

    expect(await screen.findByRole('button', { name: /edit/i })).toBeInTheDocument();
    expect(screen.queryByRole('textbox')).not.toBeInTheDocument();
  });

  it('shows a save error and keeps the draft in edit mode when the PUT request fails', async () => {
    server.use(
      http.put(`${API_URL}/api/widgets/1`, () => HttpResponse.json({ title: 'Bad Request' }, { status: 400 })),
    );
    const user = userEvent.setup();
    renderWithClient(<WidgetItem widget={textWidget} />);

    await user.click(screen.getByRole('button', { name: /edit/i }));
    await user.clear(screen.getByRole('textbox'));
    await user.type(screen.getByRole('textbox'), 'unsaved draft');
    await user.click(screen.getByRole('button', { name: /save/i }));

    expect(await screen.findByText(/failed to save/i)).toBeInTheDocument();
    expect(screen.getByRole('textbox')).toHaveValue('unsaved draft');
  });

  it('cancelling after a failed save clears the save error', async () => {
    server.use(
      http.put(`${API_URL}/api/widgets/1`, () => HttpResponse.json({ title: 'Bad Request' }, { status: 400 })),
    );
    const user = userEvent.setup();
    renderWithClient(<WidgetItem widget={textWidget} />);

    await user.click(screen.getByRole('button', { name: /edit/i }));
    await user.click(screen.getByRole('button', { name: /save/i }));
    expect(await screen.findByText(/failed to save/i)).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: /cancel/i }));

    await waitFor(() => expect(screen.queryByText(/failed to save/i)).not.toBeInTheDocument());
  });
});
