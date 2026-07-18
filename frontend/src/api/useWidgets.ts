import { useInfiniteQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { createWidget, deleteWidget, getWidgetsPage, updateWidgetText } from './widgets';
import type { WidgetType } from '../types/widget';

const WIDGETS_KEY = ['widgets'];
const PAGE_SIZE = 30;

export function useInfiniteWidgets() {
  return useInfiniteQuery({
    queryKey: WIDGETS_KEY,
    queryFn: ({ pageParam }) => getWidgetsPage(pageParam, PAGE_SIZE),
    initialPageParam: undefined as number | undefined,
    getNextPageParam: (lastPage) => (lastPage.hasMore ? (lastPage.nextCursor ?? undefined) : undefined),
  });
}

export function useCreateWidget() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (type: WidgetType) => createWidget(type),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: WIDGETS_KEY }),
  });
}

export function useUpdateWidgetText() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, text }: { id: number; text: string }) => updateWidgetText(id, text),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: WIDGETS_KEY }),
  });
}

export function useDeleteWidget() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => deleteWidget(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: WIDGETS_KEY }),
  });
}
