import { FormTextField } from '@/components/forms/FormTextField';
/**
 * The core Audit fields — exactly CreateAuditRequest/UpdateAuditRequest's
 * shape (Title, Description, Scope, Location). Planned dates, team and
 * checklist are NOT form fields on the backend: they're set via separate
 * lifecycle/assignment endpoints once the audit exists, so they're handled
 * as their own sections/actions on the edit page, not here.
 */
export function AuditForm() {
    return (<>
      <FormTextField name="title" label="Title" required/>
      <FormTextField name="scope" label="Scope" required multiline minRows={2}/>
      <FormTextField name="location" label="Location" required/>
      <FormTextField name="description" label="Description" multiline minRows={3}/>
    </>);
}
