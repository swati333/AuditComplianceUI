import { Controller, useFormContext, type FieldValues, type Path } from 'react-hook-form';
import FormControl from '@mui/material/FormControl';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import Select from '@mui/material/Select';
import FormHelperText from '@mui/material/FormHelperText';

export interface FormSelectOption {
  value: string;
  label: string;
}

export interface FormSelectProps<T extends FieldValues> {
  name: Path<T>;
  label: string;
  options: FormSelectOption[];
  required?: boolean;
  disabled?: boolean;
}

export function FormSelect<T extends FieldValues>({
  name,
  label,
  options,
  required,
  disabled,
}: FormSelectProps<T>) {
  const { control } = useFormContext<T>();
  const labelId = `${name}-label`;

  return (
    <Controller
      name={name}
      control={control}
      render={({ field, fieldState }) => (
        <FormControl
          fullWidth
          required={required}
          error={Boolean(fieldState.error)}
          disabled={disabled}
        >
          <InputLabel id={labelId}>{label}</InputLabel>
          <Select {...field} labelId={labelId} id={name} label={label} value={field.value ?? ''}>
            {options.map((option) => (
              <MenuItem key={option.value} value={option.value}>
                {option.label}
              </MenuItem>
            ))}
          </Select>
          {fieldState.error && <FormHelperText>{fieldState.error.message}</FormHelperText>}
        </FormControl>
      )}
    />
  );
}
