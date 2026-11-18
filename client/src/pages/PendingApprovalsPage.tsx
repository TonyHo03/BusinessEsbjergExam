import { useState } from 'react';
import { EmptyPanel, ErrorPanel, LoadingRows } from '../components/StatePanels';
import { usePendingApprovals } from '../hooks/usePendingApprovals';
import { accountLabel, formatDate, formatDateRange, formatDays, formatHours, initials } from '../lib/format';
import { REASON_MAX_LENGTH, type PendingRequestDto } from '../types/approvals';

export function PendingApprovalsPage() {
    const { state, busyId, actionError, reload, approve, reject } = usePendingApprovals();
    const items = state.kind === 'ready' ? state.items : [];

    return (
        <main>
            <section aria-labelledby="pending-title">
                <h1 id="pending-title">Pending approvals</h1>
                <p>Review each request, then approve or reject it. Every decision is saved.</p>

                {actionError && <p role="alert">{actionError}</p>}
                {state.kind === 'loading' && <LoadingRows label="Loading requests" />}
                {state.kind === 'error' && <ErrorPanel title="We couldn't load the requests" message={state.message} onRetry={reload} />}
                {state.kind === 'ready' && items.length === 0 && (
                    <EmptyPanel title="You're all caught up">
                        <p>There are no requests waiting for a decision. New requests will appear here.</p>
                    </EmptyPanel>)}

                {items.length > 0 && (
                    <>
                        <div>
                            <p>Waiting for a decision</p>
                            <p>{items.length} {items.length === 1 ? 'request' : 'requests'}</p>
                            <p>{formatHours(items.reduce((sum, i) => sum + i.hours, 0))} reserved</p>
                        </div>

                        <div>
                            <table>
                                <thead>
                                <tr>
                                    <th scope="col">Employee</th>
                                    <th scope="col">Request</th>
                                    <th scope="col">Decision</th>
                                </tr>
                                </thead>
                                <tbody>
                                {items.map((item) => (
                                    <RequestRow
                                        key={item.requestId}
                                        item={item}
                                        busy={busyId === item.requestId}
                                        onApprove={() => approve(item.requestId)}
                                        onReject={(reason) => reject(item.requestId, reason)}
                                    />
                                ))}
                                </tbody>
                            </table>
                        </div>
                    </>
                )}
            </section>
        </main>
    );
}

interface RequestRowProps {
    item: PendingRequestDto;
    busy: boolean;
    onApprove: () => Promise<boolean>;
    onReject: (reason: string) => Promise<boolean>;
}

function RequestRow({ item, busy, onApprove, onReject }: RequestRowProps) {
    const [rejecting, setRejecting] = useState(false);
    const [reason, setReason] = useState('');
    const [reasonError, setReasonError] = useState<string | null>(null);
    const reasonId = `reason-${item.requestId}`;

    async function confirmReject() {
        if (!reason.trim()) return setReasonError('Add a reason. The employee will see it.');
        await onReject(reason.trim());
    }

    return (
        <>
            <tr>
                <td>
                    <div>
                        <span aria-hidden="true">{initials(item.employeeName)}</span>
                        <p>{item.employeeName}</p>
                    </div>
                </td>
                <td>
                    <p>{formatDateRange(item.startDate, item.endDate)}</p>
                    <p>
                        {formatDays(item.workingDays)} · {formatHours(item.hours)} · {accountLabel(item.account)} · submitted {formatDate(item.submittedAt)}
                    </p>
                    {item.note && <p>{item.note}</p>}
                </td>
                <td>
                    {item.isOwnRequest ? (
                        <p>This is your request. Another approver is required.</p>) : (
                        <div>
                            <button type="button" disabled={busy}
                                    onClick={() => void onApprove()} aria-label={`Approve request from ${item.employeeName}`}>
                                Approve
                            </button>
                            <button type="button" disabled={busy}
                                    aria-expanded={rejecting} onClick={() => setRejecting((open) => !open)}
                                    aria-label={`Reject request from ${item.employeeName}`}>
                                Reject
                            </button>
                        </div>)}
                </td>
            </tr>
            {rejecting && !item.isOwnRequest && (
                <tr>
                    <td colSpan={3}>
                        <div>
                            <label htmlFor={reasonId}>Reason for rejecting</label>
                            <textarea id={reasonId} value={reason} maxLength={REASON_MAX_LENGTH}
                                      aria-invalid={reasonError !== null} aria-describedby={reasonError ? `${reasonId}-error` : undefined}
                                      onChange={(event) => { setReason(event.target.value); setReasonError(null); }} />
                            {reasonError && <p id={`${reasonId}-error`}>{reasonError}</p>}
                            <div>
                                <button type="button" disabled={busy} onClick={() => void confirmReject()}>
                                    Reject request
                                </button>
                                <button type="button" disabled={busy} onClick={() => setRejecting(false)}>
                                    Keep pending
                                </button>
                            </div>
                        </div>
                    </td>
                </tr>
            )}
        </>
    );
}