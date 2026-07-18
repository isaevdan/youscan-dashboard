import { apiClient } from './client';
import type { Widget, WidgetsPage, WidgetType } from '../types/widget';

export async function getWidgetsPage(cursor?: number, limit?: number): Promise<WidgetsPage> {
  const { data } = await apiClient.get<WidgetsPage>('/api/widgets', {
    params: { after: cursor, limit },
  });
  return data;
}

export async function createWidget(type: WidgetType): Promise<Widget> {
  const { data } = await apiClient.post<Widget>('/api/widgets', { type });
  return data;
}

export async function updateWidgetText(id: number, text: string): Promise<Widget> {
  const { data } = await apiClient.put<Widget>(`/api/widgets/${id}`, { text });
  return data;
}

export async function deleteWidget(id: number): Promise<void> {
  await apiClient.delete(`/api/widgets/${id}`);
}
