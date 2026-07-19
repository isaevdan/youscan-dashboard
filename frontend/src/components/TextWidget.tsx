import { useState } from 'react';
import { Alert, Button, Input, Space, Typography } from 'antd';

/**
 * Mirrors the backend's UpdateWidgetTextCommandValidator.MaxTextLength.
 * Counting caveat: the backend's MaximumLength(5000) counts UTF-16 code units,
 * while antd's maxLength/showCount counts code points, so emoji-heavy text can
 * pass this client-side counter yet still get a 400. That's acceptable: the
 * server stays the authority, and the failure path keeps the draft in edit mode.
 */
export const MAX_TEXT_LENGTH = 5000;

interface TextWidgetProps {
  text: string;
  /** Resolves when the save is persisted; rejects on failure (edit mode is kept). */
  onSave: (text: string) => Promise<unknown>;
  isSaving: boolean;
  saveError?: string;
  /** Invoked when the user cancels editing (e.g. to reset a stale save error). */
  onCancel?: () => void;
}

export function TextWidget({ text, onSave, isSaving, saveError, onCancel }: TextWidgetProps) {
  const [isEditing, setIsEditing] = useState(false);
  const [draft, setDraft] = useState(text);

  const handleSave = async () => {
    try {
      await onSave(draft);
      setIsEditing(false);
    } catch {
      // Save failed: stay in edit mode so the draft is not lost.
      // The error itself is surfaced via saveError.
    }
  };

  if (isEditing) {
    return (
      <Space orientation="vertical" style={{ width: '100%' }}>
        {saveError && <Alert type="error" message={saveError} />}
        <Input.TextArea
          value={draft}
          onChange={(e) => setDraft(e.target.value)}
          rows={3}
          maxLength={MAX_TEXT_LENGTH}
          showCount
        />
        <Space>
          <Button type="primary" loading={isSaving} disabled={isSaving} onClick={handleSave}>
            Save
          </Button>
          <Button
            disabled={isSaving}
            onClick={() => {
              setIsEditing(false);
              onCancel?.();
            }}
          >
            Cancel
          </Button>
        </Space>
      </Space>
    );
  }

  return (
    <Space orientation="vertical" style={{ width: '100%' }}>
      {saveError && <Alert type="error" message={saveError} />}
      <Typography.Paragraph>
        {text || <Typography.Text type="secondary">Empty</Typography.Text>}
      </Typography.Paragraph>
      <Button
        onClick={() => {
          setDraft(text);
          setIsEditing(true);
        }}
      >
        Edit
      </Button>
    </Space>
  );
}
