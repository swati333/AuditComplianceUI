import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import Chip from '@mui/material/Chip';
import FormControl from '@mui/material/FormControl';
import FormLabel from '@mui/material/FormLabel';
import RadioGroup from '@mui/material/RadioGroup';
import FormControlLabel from '@mui/material/FormControlLabel';
import Radio from '@mui/material/Radio';
import FormHelperText from '@mui/material/FormHelperText';
import TextField from '@mui/material/TextField';
/** One checklist question: compliant/non-compliant/not-applicable + a comment/evidence text field. */
export function ChecklistQuestion({ question, answerState, answerText, onAnswerStateChange, onAnswerTextChange, error, disabled, }) {
    const groupName = `question-${question.id}`;
    return (<Paper variant="outlined" sx={{ p: 2.5 }}>
      <Stack spacing={1.5}>
        <Stack direction="row" spacing={1} sx={{ alignItems: 'flex-start' }}>
          <Typography sx={{ flexGrow: 1 }}>{question.text}</Typography>
          {question.isMandatory && (<Chip size="small" label="Required" color="warning" variant="outlined"/>)}
        </Stack>

        <FormControl error={Boolean(error?.answerState)} disabled={disabled}>
          <FormLabel id={`${groupName}-label`} sx={{ typography: 'body2' }}>
            Answer
          </FormLabel>
          <RadioGroup row aria-labelledby={`${groupName}-label`} name={groupName} value={answerState ?? ''} onChange={(event) => onAnswerStateChange(event.target.value)}>
            <FormControlLabel value="compliant" control={<Radio />} label="Compliant"/>
            <FormControlLabel value="nonCompliant" control={<Radio />} label="Non-compliant"/>
            <FormControlLabel value="notApplicable" control={<Radio />} label="Not applicable"/>
          </RadioGroup>
          {error?.answerState && <FormHelperText>{error.answerState}</FormHelperText>}
        </FormControl>

        <TextField label="Comments / evidence" value={answerText} onChange={(event) => onAnswerTextChange(event.target.value)} error={Boolean(error?.answerText)} helperText={error?.answerText} disabled={disabled} multiline minRows={2} fullWidth slotProps={{ htmlInput: { maxLength: 4000 } }}/>
      </Stack>
    </Paper>);
}
