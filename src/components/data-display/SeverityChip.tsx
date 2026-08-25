import Chip from '@mui/material/Chip';
import { severityColor } from '@/config/theme';

export function SeverityChip({ severity }: { severity: string }) {
  return (
    <Chip
      size="small"
      label={severity}
      color={severityColor[severity] ?? 'default'}
      variant="outlined"
    />
  );
}
