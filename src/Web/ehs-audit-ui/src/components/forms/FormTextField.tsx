import { Controller, useFormContext, type FieldValues, type Path } from 'react-hook-form';
import TextField, { type TextFieldProps } from '@mui/material/TextField';

export interface FormTextFieldProps<T extends FieldValues> extends Omit<
  TextFieldProps,
  'name' | 'error'
> {
  name: Path<T>;
}

export function FormTextField<T extends FieldValues>({
  name,
  helperText,
  ...rest
}: FormTextFieldProps<T>) {
  const { control } = useFormContext<T>();

  return (
    <Controller
      name={name}
      control={control}
      render={({ field, fieldState }) => (
        <TextField
          {...field}
          {...rest}
          id={name}
          value={field.value ?? ''}
          error={Boolean(fieldState.error)}
          helperText={fieldState.error?.message ?? helperText}
          fullWidth
        />
      )}
    />
  );
}
