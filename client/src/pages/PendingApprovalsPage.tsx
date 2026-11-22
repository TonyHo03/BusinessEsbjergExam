import { useState } from 'react';
import { Check, X } from 'lucide-react';
import { EmptyPanel, ErrorPanel, LoadingRows } from '../components/StatePanels';
import { usePendingApprovals } from '../hooks/usePendingApprovals';
import { accountLabel, formatDate, formatDateRange, formatDays, formatHours, initials } from '../lib/format';
import { REASON_MAX_LENGTH, type PendingRequestDto } from '../types/approvals';
import { Label } from '@/components/ui/label.tsx';
import { Separator } from '@/components/ui/separator.tsx';
import { Card, CardContent } from '@/components/ui/card.tsx';
import { Avatar, AvatarFallback } from '@/components/ui/avatar.tsx';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table.tsx';
import { PendingTransfers } from '@/components/PendingTransfers.tsx';

export function PendingApprovalsPage() {
    const { state, busyId, actionError, reload, approve, reject } = usePendingApprovals();
    const items = state.kind === 'ready' ? state.items : [];
    // Annulleringer reserverer ikke timer – ferien er allerede trukket fra saldoen
    const reservedHours = items.filter((i) => !i.isCancellation).reduce((sum, i) => sum + i.hours, 0);

    return (
        <main className="flex flex-col w-full h-full p-12 gap-4">
            <section aria-labelledby="pending-title" className="flex flex-col gap-4">
                <h1 id="pending-title" className="text-[#0F2338] text-3xl font-normal">Pending approvals</h1>
                <Label className="text-[#516080]">Review each request, then approve or reject it. Every decision is saved.</Label>
                <Separator className="h-px" />

                {actionError && (
                    <p role="alert" className="rounded-lg border border-[#CE6B4C] bg-[#CE6B4C]/10 px-4 py-3 text-sm text-[#B5583B]">
                        {actionError}
                    </p>
                )}
                {state.kind === 'loading' && <LoadingRows label="Loading requests" />}
                {state.kind === 'error' && <ErrorPanel title="We couldn't load the requests" message={state.message} onRetry={reload} />}
                {state.kind === 'ready' && items.length === 0 && (
                    <EmptyPanel title="You're all caught up">
                        <p>There are no requests waiting for a decision. New requests will appear here.</p>
                    </EmptyPanel>
                )}

                {items.length > 0 && (
                    <>
                        <div className="flex w-full gap-4">
                            <Card className="flex-1">
                                <CardContent className="flex flex-col gap-6">
                                    <p className="text-sm text-muted-foreground">Waiting for a decision</p>
                                    <p className="text-3xl font-semibold text-[#023459]">{items.length}</p>
                                    <p className="text-xs text-muted-foreground">{items.length === 1 ? 'request' : 'requests'}</p>
                                </CardContent>
                            </Card>
                            <Card className="flex-1">
                                <CardContent className="flex flex-col gap-6">
                                    <p className="text-sm text-muted-foreground">Hours reserved</p>
                                    <p className="text-3xl font-semibold text-[#023459]">{formatHours(reservedHours)}</p>
                                    <p className="text-xs text-muted-foreground">Held until each request is decided</p>
                                </CardContent>
                            </Card>
                        </div>

                        <div className="border rounded-xl bg-card">
                            <Table>
                                <TableHeader className="bg-muted! rounded-t-xl overflow-hidden!">
                                    <TableRow>
                                        <TableHead scope="col">Employee</TableHead>
                                        <TableHead scope="col">Request</TableHead>
                                        <TableHead scope="col" className="text-right">Decision</TableHead>
                                    </TableRow>
                                </TableHeader>
                                <TableBody className="[&_td]:py-4 [&_td]:text-[#023459]">
                                    {items.map((item) => (
                                        <RequestRow
                                            key={item.requestId}
                                            item={item}
                                            busy={busyId === item.requestId}
                                            onApprove={() => approve(item.requestId)}
                                            onReject={(reason) => reject(item.requestId, reason)}
                                        />
                                    ))}
                                </TableBody>
                            </Table>
                        </div>
                    </>
                )}
            </section>

            <PendingTransfers />
        </main>
    );
}

interface RequestRowProps {
    item: PendingRequestDto;
    busy: boolean;
    onApprove: () => Promise<boolean>;
    onReject: (reason: string) => Promise<boolean>;
}

const primaryButton =
    'inline-flex h-9 items-center justify-center gap-2 rounded-lg bg-primary px-4 text-sm font-medium text-primary-foreground disabled:opacity-50';
const secondaryButton =
    'inline-flex h-9 items-center justify-center gap-2 rounded-lg border border-[#E9E1D6] bg-card px-4 text-sm font-medium text-[#023459] hover:bg-muted disabled:opacity-50';

function RequestRow({ item, busy, onApprove, onReject }: RequestRowProps) {
    const [rejecting, setRejecting] = useState(false);
    const [reason, setReason] = useState('');
    const [reasonError, setReasonError] = useState<string | null>(null);
    const reasonId = `reason-${item.requestId}`;
    const cancellation = item.isCancellation === true;

    async function confirmReject() {
        if (!reason.trim()) return setReasonError('Add a reason. The employee will see it.');
        await onReject(reason.trim());
    }

    return (
        <>
            <TableRow>
                <TableCell>
                    <div className="flex items-center gap-3">
                        <Avatar>
                            <AvatarFallback aria-hidden="true" className="bg-muted text-xs font-medium text-[#023459]">
                                {initials(item.employeeName)}
                            </AvatarFallback>
                        </Avatar>
                        <div className="flex flex-col">
                            <span className="font-medium">{item.employeeName}</span>
                            {item.isOwnRequest && <span className="text-xs text-muted-foreground">Your request</span>}
                        </div>
                    </div>
                </TableCell>
                <TableCell>
                    <div className="flex flex-col gap-1">
                        <span className="font-medium">{formatDateRange(item.startDate, item.endDate)}</span>
                        {cancellation && (
                            <span className="w-fit rounded-md bg-[#FEF3C7] px-2 py-0.5 text-xs font-medium text-[#92400E]">
                                Wants to cancel this approved holiday
                            </span>
                        )}
                        <span className="text-xs text-muted-foreground">
                            {formatDays(item.workingDays)} · {formatHours(item.hours)} · {accountLabel(item.account)} · submitted {formatDate(item.submittedAt)}
                        </span>
                        {item.note && <span className="text-sm italic text-[#516080] whitespace-normal">“{item.note}”</span>}
                    </div>
                </TableCell>
                <TableCell className="text-right">
                    <div className="flex justify-end gap-2">
                        <button type="button" disabled={busy} className={primaryButton}
                                onClick={() => void onApprove()} aria-label={`${cancellation ? 'Approve cancellation from' : 'Approve request from'} ${item.employeeName}`}>
                            <Check className="size-4" /> {cancellation ? 'Approve cancellation' : 'Approve'}
                        </button>
                        <button type="button" disabled={busy} className={secondaryButton}
                                aria-expanded={rejecting} onClick={() => setRejecting((open) => !open)}
                                aria-label={`${cancellation ? 'Keep holiday for' : 'Reject request from'} ${item.employeeName}`}>
                            <X className="size-4" /> {cancellation ? 'Keep holiday' : 'Reject'}
                        </button>
                    </div>
                </TableCell>
            </TableRow>
            {rejecting && (
                <TableRow className="bg-muted/40 hover:bg-muted/40">
                    <TableCell colSpan={3}>
                        <div className="flex flex-col gap-2">
                            <label htmlFor={reasonId} className="text-sm font-medium text-[#023459]">
                                {cancellation ? 'Why the holiday is kept' : 'Reason for rejecting'}
                            </label>
                            <textarea id={reasonId} value={reason} maxLength={REASON_MAX_LENGTH} rows={3}
                                      className="w-full rounded-lg border-2 border-[#E9E1D6] bg-card p-3 text-sm text-[#023459] outline-none focus-visible:border-[#CE6B4C]"
                                      aria-invalid={reasonError !== null} aria-describedby={reasonError ? `${reasonId}-error` : undefined}
                                      onChange={(event) => { setReason(event.target.value); setReasonError(null); }} />
                            {reasonError && <p id={`${reasonId}-error`} className="text-sm text-[#B5583B]">{reasonError}</p>}
                            <div className="flex gap-2">
                                <button type="button" disabled={busy} className={primaryButton} onClick={() => void confirmReject()}>
                                    {cancellation ? 'Keep holiday' : 'Reject request'}
                                </button>
                                <button type="button" disabled={busy} className={secondaryButton} onClick={() => setRejecting(false)}>
                                    Keep pending
                                </button>
                            </div>
                        </div>
                    </TableCell>
                </TableRow>
            )}
        </>
    );
}