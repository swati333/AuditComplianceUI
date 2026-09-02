import { FormTextField } from '@/components/forms/FormTextField';
import { FormSelect } from '@/components/forms/FormSelect';
/**
 * The core Finding fields — exactly CreateFindingRequest/UpdateFindingRequest's
 * shape (AuditId, Title, Description, Severity). Finding.Api has no
 * "responsible person", "due date", or "checklist question reference" field
 * on these requests, so this form doesn't offer them (see the phase summary
 * for why) — Root cause, comments and documents are all separate live
 * actions (their own endpoints), handled on the detail page, not here.
 */
export function FindingForm({ auditIdLocked }) {
    return (<>
      {!auditIdLocked && (<FormTextField name="auditId" label="Audit ID" required/>)}
      <FormTextField name="title" label="Title" required/>
      <FormSelect name="severity" label="Severity" required options={[
            { value: 'Low', label: 'Low' },
            { value: 'Medium', label: 'Medium' },
            { value: 'High', label: 'High' },
            { value: 'Critical', label: 'Critical' },
        ]}/>
      <FormTextField name="description" label="Description" multiline minRows={3}/>
    </>);
}
