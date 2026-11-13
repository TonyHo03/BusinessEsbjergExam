import { useCallback, useEffect, useState } from 'react';
import { approveRequest, errorMessage, getPendingApprovals, rejectRequest } from '../api/approvalsApi';
import type { PendingRequestDto } from '../types/approvals';

type LoadState =
    | { kind: 'loading' }
    | { kind: 'error'; message: string }
    | { kind: 'ready'; items: PendingRequestDto[] };

export function usePendingApprovals() {
    const [state, setState] = useState<LoadState>({ kind: 'loading' });
    const [busyId, setBusyId] = useState<string | null>(null);
    const [actionError, setActionError] = useState<string | null>(null);

    const load = useCallback(async () => {
        setState({ kind: 'loading' });
        setActionError(null);
        try {
            setState({ kind: 'ready', items: await getPendingApprovals() });
        } catch (error) {
            setState({ kind: 'error', message: errorMessage(error) });
        }
    }, []);

    useEffect(() => {
        void load();
    }, [load]);
    
    const decide = useCallback(async (requestId: string, run: () => Promise<void>) => {
        setBusyId(requestId);
        setActionError(null);
        try {
            await run();
            setState((s) => (s.kind === 'ready' ? { kind: 'ready', items: s.items.filter((i) => i.requestId !== requestId) } : s));
            return true;
        } catch (error) {
            setActionError(errorMessage(error));
            return false;
        } finally {
            setBusyId(null);
        }
    }, []);

    return {
        state,
        busyId,
        actionError,
        reload: load,
        approve: (id: string) => decide(id, () => approveRequest(id)),
        reject: (id: string, reason: string) => decide(id, () => rejectRequest(id, reason)),
    };
}