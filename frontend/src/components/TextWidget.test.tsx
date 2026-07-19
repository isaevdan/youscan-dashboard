import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { MAX_TEXT_LENGTH, TextWidget } from './TextWidget';

describe('TextWidget', () => {
  it('renders the current text', () => {
    render(<TextWidget text="hello" onSave={vi.fn()} isSaving={false} />);
    expect(screen.getByText('hello')).toBeInTheDocument();
  });

  it('shows a placeholder when text is empty', () => {
    render(<TextWidget text="" onSave={vi.fn()} isSaving={false} />);
    expect(screen.getByText(/empty/i)).toBeInTheDocument();
  });

  it('entering edit mode shows a textarea pre-filled with the current text', async () => {
    const user = userEvent.setup();
    render(<TextWidget text="hello" onSave={vi.fn()} isSaving={false} />);

    await user.click(screen.getByRole('button', { name: /edit/i }));

    expect(screen.getByRole('textbox')).toHaveValue('hello');
  });

  it('limits the textarea to the backend text length limit', async () => {
    const user = userEvent.setup();
    render(<TextWidget text="hello" onSave={vi.fn()} isSaving={false} />);

    await user.click(screen.getByRole('button', { name: /edit/i }));

    expect(screen.getByRole('textbox')).toHaveAttribute('maxlength', String(MAX_TEXT_LENGTH));
  });

  it('clicking Save calls onSave with the edited text', async () => {
    const onSave = vi.fn().mockResolvedValue(undefined);
    const user = userEvent.setup();
    render(<TextWidget text="hello" onSave={onSave} isSaving={false} />);

    await user.click(screen.getByRole('button', { name: /edit/i }));
    await user.clear(screen.getByRole('textbox'));
    await user.type(screen.getByRole('textbox'), 'updated');
    await user.click(screen.getByRole('button', { name: /save/i }));

    expect(onSave).toHaveBeenCalledWith('updated');
  });

  it('exits edit mode when the save succeeds', async () => {
    const onSave = vi.fn().mockResolvedValue(undefined);
    const user = userEvent.setup();
    render(<TextWidget text="hello" onSave={onSave} isSaving={false} />);

    await user.click(screen.getByRole('button', { name: /edit/i }));
    await user.click(screen.getByRole('button', { name: /save/i }));

    expect(await screen.findByRole('button', { name: /edit/i })).toBeInTheDocument();
    expect(screen.queryByRole('textbox')).not.toBeInTheDocument();
  });

  it('stays in edit mode and keeps the draft when the save fails', async () => {
    const onSave = vi.fn().mockRejectedValue(new Error('save failed'));
    const user = userEvent.setup();
    render(<TextWidget text="hello" onSave={onSave} isSaving={false} />);

    await user.click(screen.getByRole('button', { name: /edit/i }));
    await user.clear(screen.getByRole('textbox'));
    await user.type(screen.getByRole('textbox'), 'draft to keep');
    await user.click(screen.getByRole('button', { name: /save/i }));

    expect(screen.getByRole('textbox')).toHaveValue('draft to keep');
    expect(screen.getByRole('button', { name: /save/i })).toBeInTheDocument();
  });

  it('clicking Cancel discards changes and exits edit mode', async () => {
    const user = userEvent.setup();
    render(<TextWidget text="hello" onSave={vi.fn()} isSaving={false} />);

    await user.click(screen.getByRole('button', { name: /edit/i }));
    await user.clear(screen.getByRole('textbox'));
    await user.type(screen.getByRole('textbox'), 'discarded');
    await user.click(screen.getByRole('button', { name: /cancel/i }));

    expect(screen.getByText('hello')).toBeInTheDocument();
    expect(screen.queryByRole('textbox')).not.toBeInTheDocument();
  });

  it('clicking Cancel invokes onCancel so the caller can reset save errors', async () => {
    const onCancel = vi.fn();
    const user = userEvent.setup();
    render(<TextWidget text="hello" onSave={vi.fn()} isSaving={false} onCancel={onCancel} />);

    await user.click(screen.getByRole('button', { name: /edit/i }));
    await user.click(screen.getByRole('button', { name: /cancel/i }));

    expect(onCancel).toHaveBeenCalledTimes(1);
  });

  it('shows an error message when saveError is set', () => {
    render(<TextWidget text="hello" onSave={vi.fn()} isSaving={false} saveError="Failed to save" />);
    expect(screen.getByText('Failed to save')).toBeInTheDocument();
  });

  it('shows the save error inside the edit view', async () => {
    const user = userEvent.setup();
    render(<TextWidget text="hello" onSave={vi.fn()} isSaving={false} saveError="Failed to save" />);

    await user.click(screen.getByRole('button', { name: /edit/i }));

    expect(screen.getByRole('textbox')).toBeInTheDocument();
    expect(screen.getByText('Failed to save')).toBeInTheDocument();
  });

  it('disables Save while isSaving', async () => {
    const user = userEvent.setup();
    render(<TextWidget text="hello" onSave={vi.fn()} isSaving={true} />);

    await user.click(screen.getByRole('button', { name: /edit/i }));

    expect(screen.getByRole('button', { name: /save/i })).toBeDisabled();
  });
});
