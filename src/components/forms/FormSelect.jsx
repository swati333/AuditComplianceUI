import { useId } from 'react';
import { Controller, useFormContext } from 'react-hook-form';
import FormControl from '@mui/material/FormControl';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import Select from '@mui/material/Select';
import FormHelperText from '@mui/material/FormHelperText';
export function FormSelect({ name, label, options, required, disabled, }) {
    const { control } = useFormContext();
    const uid = useId();
    const labelId = `${name}-${uid}-label`;
    return (<Controller name={name} control={control} render={({ field, fieldState }) => (<FormControl fullWidth required={required} error={Boolean(fieldState.error)} disabled={disabled}>
          <InputLabel id={labelId}>{label}</InputLabel>
          <Select {...field} labelId={labelId} id={`${name}-${uid}`} label={label} value={field.value ?? ''}>
            {options.map((option) => (<MenuItem key={option.value} value={option.value}>
                {option.label}
              </MenuItem>))}
          </Select>
          {fieldState.error && <FormHelperText>{fieldState.error.message}</FormHelperText>}
        </FormControl>)}/>);
}
