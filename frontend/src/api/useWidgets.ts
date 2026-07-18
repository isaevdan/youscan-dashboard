import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createWidget, deleteWidget, getWidgets, updateWidgetText } from './widgets';
import type { WidgetType } from '../types/widget';

const WIDGETS_KEY = ['widgets'];

export function useWidgets() {
  return useQuery({ queryKey: WIDGETS_KEY, queryFn: getWidgets });
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
