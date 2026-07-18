import { useState } from 'react';
import { Alert, Button, Input, Space, Typography } from 'antd';

interface TextWidgetProps {
  text: string;
  onSave: (text: string) => void;
  isSaving: boolean;
  saveError?: string;
}

export function TextWidget({ text, onSave, isSaving, saveError }: TextWidgetProps) {
  const [isEditing, setIsEditing] = useState(false);
  const [draft, setDraft] = useState(text);

  if (isEditing) {
    return (
      <Space orientation="vertical" style={{ width: '100%' }}>
        <Input.TextArea value={draft} onChange={(e) => setDraft(e.target.value)} rows={3} />
        <Space>
          <Button
            type="primary"
            loading={isSaving}
            disabled={isSaving}
            onClick={() => {
              onSave(draft);
              setIsEditing(false);
            }}
          >
            Save
          </Button>
          <Button onClick={() => setIsEditing(false)}>Cancel</Button>
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
