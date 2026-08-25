import type { FetchBaseQueryError } from '@reduxjs/toolkit/query';
import type { SerializedError } from '@reduxjs/toolkit';
import { isProblemDetails, type ProblemDetails } from '@/types/problemDetails';

/**
 * Central ProblemDetails mapping (CLAUDE.md §9): every RTK Query error,
 * whatever its origin (a well-formed ProblemDetails body, a network failure,
 * a timeout, an unparsable response), is normalized to this one shape so
 * every feature renders errors the same way instead of each hand-rolling its
 * own switch over FetchBaseQueryError's status field.
 */
export function toProblemDetails(
  error: FetchBaseQueryError | SerializedError | undefined,
): ProblemDetails {
  if (!error) {
    return { title: 'Unknown error', errorCode: 'UNKNOWN_ERROR' };
  }

  if ('status' in error) {
    if (isProblemDetails(error.data)) {
      return error.data;
    }

    if (error.status === 'FETCH_ERROR') {
      return {
        title: 'Network error',
        detail: 'Could not reach the server. Check your connection and try again.',
        errorCode: 'NETWORK_ERROR',
      };
    }

    if (error.status === 'TIMEOUT_ERROR') {
      return { title: 'Request timed out', errorCode: 'TIMEOUT_ERROR' };
    }

    if (error.status === 'PARSING_ERROR') {
      return { title: 'Unexpected response from server', errorCode: 'PARSING_ERROR' };
    }

    return {
      title: 'Request failed',
      status: typeof error.status === 'number' ? error.status : undefined,
      errorCode: 'REQUEST_FAILED',
    };
  }

  return {
    title: error.message ?? 'Unexpected error',
    errorCode: error.code ?? 'UNEXPECTED_ERROR',
  };
}

export function getErrorMessage(problem: ProblemDetails): string {
  return problem.detail ?? problem.title ?? 'Something went wrong. Please try again.';
}
