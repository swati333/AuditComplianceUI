import { describe, expect, it, vi } from 'vitest';
import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { renderWithProviders } from '@/test/render';
import { DocumentUploader } from '@/features/audits/components/DocumentUploader';
describe('DocumentUploader', () => {
    it('shows the empty message when there are no documents', () => {
        renderWithProviders(<DocumentUploader documents={[]} emptyMessage="Nothing here."/>);
        expect(screen.getByText('Nothing here.')).toBeInTheDocument();
    });
    it('lists documents by name', () => {
        renderWithProviders(<DocumentUploader documents={[
                {
                    id: '1',
                    fileName: 'inspection.pdf',
                    sizeBytes: 2048,
                    createdDate: '2026-08-01T00:00:00Z',
                },
            ]}/>);
        expect(screen.getByText('inspection.pdf')).toBeInTheDocument();
    });
    it('disables the upload button and shows a message when no onUpload handler is given', () => {
        renderWithProviders(<DocumentUploader documents={[]} disabledMessage="Not supported yet."/>);
        expect(screen.getByRole('button', { name: /upload document/i })).toBeDisabled();
        expect(screen.getByText('Not supported yet.')).toBeInTheDocument();
    });
    it('calls onUpload with the selected file', async () => {
        const user = userEvent.setup();
        const onUpload = vi.fn();
        renderWithProviders(<DocumentUploader documents={[]} onUpload={onUpload}/>);
        const file = new File(['content'], 'evidence.png', { type: 'image/png' });
        const input = document.querySelector('input[type="file"]');
        await user.upload(input, file);
        expect(onUpload).toHaveBeenCalledWith(file);
    });
});
