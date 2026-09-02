import { useRef } from 'react';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Typography from '@mui/material/Typography';
import AttachFileOutlinedIcon from '@mui/icons-material/AttachFileOutlined';
import UploadFileOutlinedIcon from '@mui/icons-material/UploadFileOutlined';
import { formatDate } from '@/utils/formatDate';
function formatSize(bytes) {
    if (bytes < 1024)
        return `${bytes} B`;
    if (bytes < 1024 * 1024)
        return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}
/**
 * Generic document list + picker, backend-agnostic (CLAUDE.md §10: real
 * uploads go through short-lived SAS/API-mediated endpoints the caller
 * wires via `onUpload`). Renders disabled when no `onUpload` is supplied —
 * used that way on the Audit detail page, since Audit.Api has no document
 * endpoints yet (unlike Finding.Api, which does).
 */
export function DocumentUploader({ documents, onUpload, disabledMessage = 'Document upload is not available yet.', emptyMessage = 'No documents attached.', }) {
    const inputRef = useRef(null);
    return (<Box>
      {documents.length === 0 ? (<Typography color="text.secondary" sx={{ mb: 2 }}>
          {emptyMessage}
        </Typography>) : (<List dense sx={{ mb: 1 }}>
          {documents.map((doc) => (<ListItem key={doc.id} disableGutters>
              <ListItemIcon sx={{ minWidth: 32 }}>
                <AttachFileOutlinedIcon fontSize="small"/>
              </ListItemIcon>
              <ListItemText primary={doc.fileName} secondary={`${formatSize(doc.sizeBytes)} · ${formatDate(doc.createdDate)}`}/>
            </ListItem>))}
        </List>)}

      <input ref={inputRef} type="file" hidden onChange={(event) => {
            const file = event.target.files?.[0];
            if (file && onUpload)
                onUpload(file);
            event.target.value = '';
        }}/>
      <Button size="small" variant="outlined" startIcon={<UploadFileOutlinedIcon />} onClick={() => inputRef.current?.click()} disabled={!onUpload}>
        Upload document
      </Button>
      {!onUpload && (<Typography variant="caption" color="text.secondary" component="div" sx={{ mt: 0.5 }}>
          {disabledMessage}
        </Typography>)}
    </Box>);
}
