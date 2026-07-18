import type { ReactElement } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { server } from '../test/msw/server';
import { Dashboard } from './Dashboard';

const API_URL = 'http://localhost:8080';

function renderWithClient(ui: ReactElement) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });
  return render(<QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>);
}

function emptyPage() {
  return HttpResponse.json({ items: [], hasMore: false, nextCursor: null });
}

describe('Dashboard', () => {
  it('starts empty and shows the empty state after loading', async () => {
    server.use(http.get(`${API_URL}/api/widgets`, emptyPage));

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
    server.use(http.get(`${API_URL}/api/widgets`, emptyPage));

    renderWithClient(<Dashboard />);
    await screen.findByText(/no widgets/i);

    expect(screen.getByRole('button', { name: /add line chart/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /add bar chart/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /add text/i })).toBeInTheDocument();
  });

  it('clicking Add Text creates a widget and it appears in the grid', async () => {
    let widgets: unknown[] = [];
    server.use(
      http.get(`${API_URL}/api/widgets`, () =>
        HttpResponse.json({ items: widgets, hasMore: false, nextCursor: null }),
      ),
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

  describe('infinite scroll', () => {
    let capturedCallback: IntersectionObserverCallback | undefined;
    let originalIntersectionObserver: typeof IntersectionObserver;

    afterEach(() => {
      globalThis.IntersectionObserver = originalIntersectionObserver;
      capturedCallback = undefined;
    });

    function mockIntersectionObserver() {
      originalIntersectionObserver = globalThis.IntersectionObserver;
      class MockIntersectionObserver {
        constructor(callback: IntersectionObserverCallback) {
          capturedCallback = callback;
        }
        observe = vi.fn();
        disconnect = vi.fn();
        unobserve = vi.fn();
        takeRecords = vi.fn(() => []);
        root = null;
        rootMargin = '';
        thresholds = [];
      }
      globalThis.IntersectionObserver = MockIntersectionObserver as unknown as typeof IntersectionObserver;
    }

    it('loads the next page when the sentinel scrolls into view', async () => {
      mockIntersectionObserver();

      const firstPageItems = [
        { id: 1, type: 'Text', row: 0, column: 0, data: { text: 'widget-1' } },
        { id: 2, type: 'Text', row: 0, column: 1, data: { text: 'widget-2' } },
      ];
      const secondPageItems = [{ id: 3, type: 'Text', row: 0, column: 2, data: { text: 'widget-3' } }];

      server.use(
        http.get(`${API_URL}/api/widgets`, ({ request }) => {
          const after = new URL(request.url).searchParams.get('after');
          if (after === '2') {
            return HttpResponse.json({ items: secondPageItems, hasMore: false, nextCursor: null });
          }
          return HttpResponse.json({ items: firstPageItems, hasMore: true, nextCursor: 2 });
        }),
      );

      renderWithClient(<Dashboard />);

      await screen.findByText('widget-1');
      expect(screen.queryByText('widget-3')).not.toBeInTheDocument();

      capturedCallback?.(
        [{ isIntersecting: true } as IntersectionObserverEntry],
        {} as IntersectionObserver,
      );

      expect(await screen.findByText('widget-3')).toBeInTheDocument();
    });

    it('does not observe a sentinel when there are no more pages', async () => {
      mockIntersectionObserver();
      server.use(http.get(`${API_URL}/api/widgets`, emptyPage));

      renderWithClient(<Dashboard />);
      await screen.findByText(/no widgets/i);

      expect(capturedCallback).toBeUndefined();
    });
  });

  it('does not show the back-to-top button while the page is still at the top', async () => {
    server.use(http.get(`${API_URL}/api/widgets`, emptyPage));

    renderWithClient(<Dashboard />);
    await screen.findByText(/no widgets/i);

    // FloatButton.BackTop only mounts once the page has scrolled past its
    // visibilityHeight - this stays hidden at the default scroll position.
    // (The scroll-triggered show/hide itself is exercised manually in a real
    // browser: antd's threshold check compares the scroll event's `target`
    // to `window` by reference, which jsdom under Vitest does not preserve
    // across a dispatched `scroll` event, making that transition unreliable
    // to simulate here.)
    expect(document.querySelector('.ant-float-btn')).not.toBeInTheDocument();
  });
});
