import { useId } from 'react';
import { Controller, useFormContext } from 'react-hook-form';
import TextField from '@mui/material/TextField';
export function FormTextField({ name, helperText, ...rest }) {
    const { control } = useFormContext();
    const uid = useId();
    return (<Controller name={name} control={control} render={({ field, fieldState }) => (<TextField {...field} {...rest} id={`${name}-${uid}`} value={field.value ?? ''} error={Boolean(fieldState.error)} helperText={fieldState.error?.message ?? helperText} fullWidth/>)}/>);
}
