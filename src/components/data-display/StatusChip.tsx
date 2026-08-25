import Chip from '@mui/material/Chip';
import { statusColor } from '@/config/theme';

function splitPascalCase(value: string): string {
  return value.replace(/([a-z0-9])([A-Z])/g, '$1 $2');
}

export function StatusChip({ status }: { status: string }) {
  return (
    <Chip size="small" label={splitPascalCase(status)} color={statusColor[status] ?? 'default'} />
  );
}
