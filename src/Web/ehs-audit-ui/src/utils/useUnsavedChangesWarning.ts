import { useCallback } from 'react';
import { useBeforeUnload, useBlocker } from 'react-router';

/**
 * Warns before losing unsaved form edits — both for in-app navigation (via
 * the router's blocker, resolved through a confirmation dialog the caller
 * renders) and for closing/refreshing the tab (the browser's native prompt).
 */
export function useUnsavedChangesWarning(isDirty: boolean) {
  useBeforeUnload(
    useCallback(
      (event: BeforeUnloadEvent) => {
        if (isDirty) {
          event.preventDefault();
        }
      },
      [isDirty],
    ),
  );

  return useBlocker(
    useCallback(
      ({ currentLocation, nextLocation }) =>
        isDirty && currentLocation.pathname !== nextLocation.pathname,
      [isDirty],
    ),
  );
}
