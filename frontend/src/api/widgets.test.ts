import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { server } from '../test/msw/server';
import { createWidget, deleteWidget, getWidgets, updateWidgetText } from './widgets';

const API_URL = 'http://localhost:8080';

describe('widgets api', () => {
  it('getWidgets fetches and returns the widget list', async () => {
    server.use(
      http.get(`${API_URL}/api/widgets`, () =>
        HttpResponse.json([
          { id: 1, type: 'Text', row: 0, column: 0, data: { text: 'hi' } },
        ]),
      ),
    );

    const widgets = await getWidgets();

    expect(widgets).toHaveLength(1);
    expect(widgets[0].id).toBe(1);
  });

  it('createWidget posts the type and returns the created widget', async () => {
    let receivedBody: unknown;
    server.use(
      http.post(`${API_URL}/api/widgets`, async ({ request }) => {
        receivedBody = await request.json();
        return HttpResponse.json(
          { id: 2, type: 'LineChart', row: 0, column: 1, data: { points: [] } },
          { status: 201 },
        );
      }),
    );

    const widget = await createWidget('LineChart');

    expect(receivedBody).toEqual({ type: 'LineChart' });
    expect(widget.id).toBe(2);
  });

  it('updateWidgetText puts the text and returns the updated widget', async () => {
    let receivedBody: unknown;
    server.use(
      http.put(`${API_URL}/api/widgets/3`, async ({ request }) => {
        receivedBody = await request.json();
        return HttpResponse.json({ id: 3, type: 'Text', row: 0, column: 2, data: { text: 'new' } });
      }),
    );

    const widget = await updateWidgetText(3, 'new');

    expect(receivedBody).toEqual({ text: 'new' });
    expect(widget.data).toEqual({ text: 'new' });
  });

  it('deleteWidget sends a DELETE request', async () => {
    let called = false;
    server.use(
      http.delete(`${API_URL}/api/widgets/4`, () => {
        called = true;
        return new HttpResponse(null, { status: 204 });
      }),
    );

    await deleteWidget(4);

    expect(called).toBe(true);
  });
});
