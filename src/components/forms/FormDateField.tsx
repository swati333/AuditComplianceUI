import { Controller, useFormContext, type FieldValues, type Path } from 'react-hook-form';
import TextField from '@mui/material/TextField';
import { format, parseISO, isValid } from 'date-fns';

/** Plain HTML date input (no MUI X Date Pickers dependency); react-hook-form field value is always an ISO string or undefined. */
export function FormDateField<T extends FieldValues>({
  name,
  label,
  required,
}: {
  name: Path<T>;
  label: string;
  required?: boolean;
}) {
  const { control } = useFormContext<T>();

  return (
    <Controller
      name={name}
      control={control}
      render={({ field, fieldState }) => {
        const dateValue =
          field.value && isValid(parseISO(field.value))
            ? format(parseISO(field.value), 'yyyy-MM-dd')
            : '';

        return (
          <TextField
            id={name}
            label={label}
            type="date"
            required={required}
            value={dateValue}
            onChange={(event) =>
              field.onChange(event.target.value ? new Date(event.target.value).toISOString() : '')
            }
            onBlur={field.onBlur}
            error={Boolean(fieldState.error)}
            helperText={fieldState.error?.message}
            slotProps={{ inputLabel: { shrink: true } }}
            fullWidth
          />
        );
      }}
    />
  );
}
