import { describe, expect, it } from 'vitest';
import { getErrorMessage, toProblemDetails } from '@/services/problemDetails';

describe('toProblemDetails', () => {
  it('passes through a well-formed ProblemDetails response body', () => {
    const result = toProblemDetails({
      status: 404,
      data: {
        title: 'Resource not found',
        detail: 'Audit 123 was not found.',
        errorCode: 'AUDIT_NOT_FOUND',
      },
    });

    expect(result).toEqual({
      title: 'Resource not found',
      detail: 'Audit 123 was not found.',
      errorCode: 'AUDIT_NOT_FOUND',
    });
  });

  it('maps a network failure to a stable error code', () => {
    const result = toProblemDetails({ status: 'FETCH_ERROR', error: 'Failed to fetch' });

    expect(result.errorCode).toBe('NETWORK_ERROR');
    expect(result.title).toBe('Network error');
  });

  it('falls back to a generic message when the error is undefined', () => {
    expect(toProblemDetails(undefined).errorCode).toBe('UNKNOWN_ERROR');
  });
});

describe('getErrorMessage', () => {
  it('prefers detail over title', () => {
    expect(getErrorMessage({ title: 'Conflict', detail: 'Row version mismatch.' })).toBe(
      'Row version mismatch.',
    );
  });

  it('falls back to title when detail is missing', () => {
    expect(getErrorMessage({ title: 'Conflict' })).toBe('Conflict');
  });

  it('falls back to a generic message when both are missing', () => {
    expect(getErrorMessage({})).toBe('Something went wrong. Please try again.');
  });
});
