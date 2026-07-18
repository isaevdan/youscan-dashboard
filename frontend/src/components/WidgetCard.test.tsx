import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { WidgetCard } from './WidgetCard';

describe('WidgetCard', () => {
  it('renders title and children', () => {
    render(
      <WidgetCard title="Text" onDelete={vi.fn()} isDeleting={false}>
        <div>content</div>
      </WidgetCard>,
    );

    expect(screen.getByText('Text')).toBeInTheDocument();
    expect(screen.getByText('content')).toBeInTheDocument();
  });

  it('calls onDelete after confirming the delete popconfirm', async () => {
    const onDelete = vi.fn();
    const user = userEvent.setup();
    render(
      <WidgetCard title="Text" onDelete={onDelete} isDeleting={false}>
        <div>content</div>
      </WidgetCard>,
    );

    await user.click(screen.getByRole('button', { name: /delete/i }));
    await user.click(await screen.findByRole('button', { name: 'Yes' }));

    expect(onDelete).toHaveBeenCalledTimes(1);
  });

  it('does not call onDelete when the confirmation is declined', async () => {
    const onDelete = vi.fn();
    const user = userEvent.setup();
    render(
      <WidgetCard title="Text" onDelete={onDelete} isDeleting={false}>
        <div>content</div>
      </WidgetCard>,
    );

    await user.click(screen.getByRole('button', { name: /delete/i }));
    await user.click(await screen.findByRole('button', { name: 'No' }));

    expect(onDelete).not.toHaveBeenCalled();
  });

  it('shows an error alert when deleteError is set', () => {
    render(
      <WidgetCard title="Text" onDelete={vi.fn()} isDeleting={false} deleteError="Failed to delete">
        <div>content</div>
      </WidgetCard>,
    );

    expect(screen.getByText('Failed to delete')).toBeInTheDocument();
  });
});
