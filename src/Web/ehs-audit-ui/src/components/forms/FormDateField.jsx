import { useId } from 'react';
import { Controller, useFormContext } from 'react-hook-form';
import TextField from '@mui/material/TextField';
import { format, parseISO, isValid } from 'date-fns';
/** Plain HTML date input (no MUI X Date Pickers dependency); react-hook-form field value is always an ISO string or undefined. */
export function FormDateField({ name, label, required, }) {
    const { control } = useFormContext();
    const uid = useId();
    return (<Controller name={name} control={control} render={({ field, fieldState }) => {
            const dateValue = field.value && isValid(parseISO(field.value))
                ? format(parseISO(field.value), 'yyyy-MM-dd')
                : '';
            return (<TextField id={`${name}-${uid}`} label={label} type="date" required={required} value={dateValue} onChange={(event) => field.onChange(event.target.value ? new Date(event.target.value).toISOString() : '')} onBlur={field.onBlur} error={Boolean(fieldState.error)} helperText={fieldState.error?.message} slotProps={{ inputLabel: { shrink: true } }} fullWidth/>);
        }}/>);
}
