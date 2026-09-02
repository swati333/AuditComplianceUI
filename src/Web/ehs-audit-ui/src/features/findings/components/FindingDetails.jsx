import { useState } from 'react';
import { useNavigate, Link as RouterLink } from 'react-router';
import { FormProvider, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Chip from '@mui/material/Chip';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import Grid from '@mui/material/Grid';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import EditOutlinedIcon from '@mui/icons-material/EditOutlined';
import HistoryOutlinedIcon from '@mui/icons-material/HistoryOutlined';
import { FindingStatusChip } from '@/features/findings/components/FindingStatusChip';
import { FindingLifecycleActions } from '@/features/findings/components/FindingLifecycleActions';
import { RootCauseSection } from '@/features/findings/components/RootCauseSection';
import { SeverityChip } from '@/components/data-display/SeverityChip';
import { DocumentUploader } from '@/features/audits/components/DocumentUploader';
import { commentSchema, addDocumentSchema, } from '@/features/findings/schemas';
import { useAddFindingCommentMutation, useAddFindingDocumentMutation, } from '@/features/findings/api';
import { FormTextField } from '@/components/forms/FormTextField';
import { RequirePolicy } from '@/auth/RequirePolicy';
import { useAuth } from '@/auth/useAuth';
import { toProblemDetails, getErrorMessage } from '@/services/problemDetails';
import { formatDateTime } from '@/utils/formatDate';
function CommentSection({ findingId, comments, }) {
    const { account } = useAuth();
    const [addComment, { isLoading }] = useAddFindingCommentMutation();
    const form = useForm({
        resolver: zodResolver(commentSchema),
        defaultValues: { text: '' },
    });
    async function onSubmit(values) {
        if (!account)
            return;
        try {
            await addComment({
                id: findingId,
                body: {
                    authorId: account.localAccountId,
                    authorName: account.name ?? account.username,
                    text: values.text,
                },
            }).unwrap();
            form.reset();
        }
        catch {
            // Field-level errors aren't expected here; RTK Query keeps the failed comment text in the form for retry.
        }
    }
    return (<Paper variant="outlined" sx={{ p: 3 }}>
      <Typography variant="h6" gutterBottom>
        Comments
      </Typography>
      {comments.length === 0 ? (<Typography color="text.secondary">No comments yet.</Typography>) : (<List dense>
          {comments.map((comment) => (<ListItem key={comment.id} disableGutters alignItems="flex-start">
              <ListItemText primary={comment.text} secondary={`${comment.authorName} · ${formatDateTime(comment.createdDate)}`}/>
            </ListItem>))}
        </List>)}
      <FormProvider {...form}>
        <Stack component="form" direction="row" spacing={1} onSubmit={form.handleSubmit(onSubmit)} noValidate sx={{ mt: 2 }}>
          <FormTextField name="text" label="Add a comment" size="small"/>
          <Button type="submit" variant="outlined" disabled={isLoading} sx={{ flexShrink: 0 }}>
            Post
          </Button>
        </Stack>
      </FormProvider>
    </Paper>);
}
/**
 * Records metadata for an already-uploaded file (AddDocumentRequest — there's
 * no blob-upload/SAS endpoint yet, CLAUDE.md §10) using the real file's name/
 * type/size, prompting only for the blob reference a real upload flow would
 * otherwise supply automatically.
 */
function AddDocumentDialog({ findingId, file, onClose, }) {
    const [addDocument, { isLoading, error }] = useAddFindingDocumentMutation();
    const form = useForm({
        resolver: zodResolver(addDocumentSchema),
        values: file
            ? {
                fileName: file.name,
                contentType: file.type || 'application/octet-stream',
                sizeBytes: file.size,
                blobReference: '',
            }
            : undefined,
    });
    async function onSubmit(values) {
        try {
            await addDocument({ id: findingId, body: values }).unwrap();
            onClose();
        }
        catch {
            // Surfaced via `error` below.
        }
    }
    return (<Dialog open={file !== null} onClose={onClose} fullWidth maxWidth="xs">
      <DialogTitle>Add document</DialogTitle>
      <FormProvider {...form}>
        <Stack component="form" onSubmit={form.handleSubmit(onSubmit)} noValidate>
          <DialogContent>
            <Stack spacing={2}>
              {error && <Alert severity="error">{getErrorMessage(toProblemDetails(error))}</Alert>}
              <Typography variant="body2" color="text.secondary">
                {file?.name} · {file ? Math.round(file.size / 1024) : 0} KB
              </Typography>
              <FormTextField name="blobReference" label="Blob reference" required helperText="No upload endpoint exists yet — enter the storage reference for this file."/>
            </Stack>
          </DialogContent>
          <DialogActions>
            <Button onClick={onClose}>Cancel</Button>
            <Button type="submit" variant="contained" disabled={isLoading}>
              Add
            </Button>
          </DialogActions>
        </Stack>
      </FormProvider>
    </Dialog>);
}
export function FindingDetails({ finding }) {
    const navigate = useNavigate();
    const [pendingUpload, setPendingUpload] = useState(null);
    const requiresAction = finding.severity === 'High' || finding.severity === 'Critical';
    return (<Stack spacing={3}>
      {finding.severity === 'Critical' && (<Alert severity="error" role="alert">
          This is a <strong>Critical</strong> finding — CLAUDE.md §2 requires it to trigger
          immediate notification and at least one corrective action.
        </Alert>)}

      <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: 2 }}>
        <Box>
          <Typography variant="h4" component="h1">
            {finding.title}
          </Typography>
          <Stack direction="row" spacing={1} sx={{ alignItems: 'center', mt: 1, flexWrap: 'wrap' }}>
            <FindingStatusChip status={finding.status}/>
            <SeverityChip severity={finding.severity}/>
            <Button size="small" component={RouterLink} to={`/audits/${finding.auditId}`}>
              View source audit
            </Button>
          </Stack>
        </Box>
        <Stack direction="row" spacing={1.5}>
          <Button variant="outlined" startIcon={<HistoryOutlinedIcon />} onClick={() => navigate(`/findings/${finding.id}/history`)}>
            History
          </Button>
          <RequirePolicy policy="CanManageFindings">
            <Button variant="outlined" startIcon={<EditOutlinedIcon />} onClick={() => navigate(`/findings/${finding.id}/edit`)}>
              Edit
            </Button>
          </RequirePolicy>
        </Stack>
      </Stack>

      <FindingLifecycleActions finding={finding}/>

      <Grid container spacing={3}>
        <Grid size={{ xs: 12, md: 8 }}>
          <Paper variant="outlined" sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Details
            </Typography>
            {finding.description && <Typography>{finding.description}</Typography>}
            {requiresAction && (<Chip sx={{ mt: 1.5 }} size="small" color="warning" variant="outlined" label={`${finding.severity} findings require a corrective action`}/>)}
          </Paper>

          <Box sx={{ mt: 3 }}>
            <RootCauseSection finding={finding}/>
          </Box>

          <Box sx={{ mt: 3 }}>
            <CommentSection findingId={finding.id} comments={finding.comments}/>
          </Box>
        </Grid>

        <Grid size={{ xs: 12, md: 4 }}>
          <Paper variant="outlined" sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Documents
            </Typography>
            <DocumentUploader documents={finding.documents} onUpload={(file) => setPendingUpload(file)} emptyMessage="No documents attached."/>
          </Paper>

          <Paper variant="outlined" sx={{ p: 3, mt: 3 }}>
            <Typography variant="h6" gutterBottom>
              Corrective actions
            </Typography>
            <Typography color="text.secondary">
              Action plan tracking isn&apos;t available yet — the Action Plan service hasn&apos;t
              shipped its API.
            </Typography>
          </Paper>
        </Grid>
      </Grid>

      <AddDocumentDialog findingId={finding.id} file={pendingUpload} onClose={() => setPendingUpload(null)}/>
    </Stack>);
}
