import { describe, expect, it, vi } from 'vitest';
import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { renderWithProviders } from '@/test/render';
import { ChecklistQuestion } from '@/features/audits/components/ChecklistQuestion';
const question = { id: 'q1', text: 'Are exits clear?', isMandatory: true, displayOrder: 1 };
describe('ChecklistQuestion', () => {
    it('shows a Required badge for mandatory questions', () => {
        renderWithProviders(<ChecklistQuestion question={question} answerState={undefined} answerText="" onAnswerStateChange={vi.fn()} onAnswerTextChange={vi.fn()}/>);
        expect(screen.getByText('Required')).toBeInTheDocument();
    });
    it('calls onAnswerStateChange when a radio option is selected', async () => {
        const user = userEvent.setup();
        const onAnswerStateChange = vi.fn();
        renderWithProviders(<ChecklistQuestion question={question} answerState={undefined} answerText="" onAnswerStateChange={onAnswerStateChange} onAnswerTextChange={vi.fn()}/>);
        await user.click(screen.getByRole('radio', { name: 'Non-compliant' }));
        expect(onAnswerStateChange).toHaveBeenCalledWith('nonCompliant');
    });
    it('calls onAnswerTextChange as comments are typed', async () => {
        const user = userEvent.setup();
        const onAnswerTextChange = vi.fn();
        renderWithProviders(<ChecklistQuestion question={question} answerState="compliant" answerText="" onAnswerStateChange={vi.fn()} onAnswerTextChange={onAnswerTextChange}/>);
        await user.type(screen.getByLabelText(/comments/i), 'x');
        expect(onAnswerTextChange).toHaveBeenCalledWith('x');
    });
    it('renders validation errors', () => {
        renderWithProviders(<ChecklistQuestion question={question} answerState={undefined} answerText="" onAnswerStateChange={vi.fn()} onAnswerTextChange={vi.fn()} error={{ answerState: 'This question is required', answerText: 'A comment is required' }}/>);
        expect(screen.getByText('This question is required')).toBeInTheDocument();
        expect(screen.getByText('A comment is required')).toBeInTheDocument();
    });
});
