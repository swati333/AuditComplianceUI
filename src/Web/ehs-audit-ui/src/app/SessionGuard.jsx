import { useEffect } from 'react';
import { useNavigate } from 'react-router';
import { useAppDispatch, useAppSelector } from '@/app/hooks';
import { sessionAcknowledged } from '@/app/sessionSlice';
/**
 * Reacts to the 401/403 state baseApi dispatches into the session slice.
 * Mounted once inside AppShell (every protected page can issue authenticated
 * API calls) so no feature page has to handle auth failures itself.
 */
export function SessionGuard() {
    const status = useAppSelector((state) => state.session.status);
    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    useEffect(() => {
        if (status === 'unauthorized') {
            dispatch(sessionAcknowledged());
            navigate('/login', { replace: true });
        }
        else if (status === 'forbidden') {
            dispatch(sessionAcknowledged());
            navigate('/unauthorized', { replace: true });
        }
    }, [status, dispatch, navigate]);
    return null;
}
