import {Fragment, useEffect, useState} from "react";
import {Check, X} from "lucide-react";
import {api} from "@/api/client.ts";
import {type PendingTransferDto} from "@/api/Api.ts";
import {formatDate} from "@/lib/format.ts";
import {Table, TableBody, TableCell, TableHead, TableHeader, TableRow} from "@/components/ui/table.tsx";

const primaryButton =
    "inline-flex h-9 items-center justify-center gap-2 rounded-lg bg-primary px-4 text-sm font-medium text-primary-foreground disabled:opacity-50";
const secondaryButton =
    "inline-flex h-9 items-center justify-center gap-2 rounded-lg border border-[#E9E1D6] bg-card px-4 text-sm font-medium text-[#023459] hover:bg-muted disabled:opacity-50";

// Overførsler, der venter på bogholderiet. Vises under ferieansøgningerne på Pending approvals
export function PendingTransfers() {
    const [items, setItems] = useState<PendingTransferDto[]>([]);
    const [busyId, setBusyId] = useState<string | null>(null);
    const [rejectingId, setRejectingId] = useState<string | null>(null);
    const [reason, setReason] = useState("");
    const [error, setError] = useState<string | null>(null);

    const load = () => {
        api.api.transfersGetPending().then(setItems).catch(() => setError("Could not load transfer requests."));
    };

    useEffect(load, []);

    const decide = async (id: string, action: () => Promise<unknown>) => {
        setBusyId(id);
        setError(null);
        try {
            await action();
            setRejectingId(null);
            setReason("");
            load();
        } catch (err: any) {
            const problem = await err?.json?.().catch(() => null);
            setError(problem?.detail ?? "Could not save the decision.");
        } finally {
            setBusyId(null);
        }
    };

    if (items.length === 0 && !error) return null;

    return (
        <section aria-labelledby="transfers-title" className="flex flex-col gap-4 mt-4">
            <h2 id="transfers-title" className="text-[#0F2338] text-xl font-normal">Transfer requests</h2>

            {error && (
                <p role="alert" className="rounded-lg border border-[#CE6B4C] bg-[#CE6B4C]/10 px-4 py-3 text-sm text-[#B5583B]">{error}</p>
            )}

            {items.length > 0 && (
                <div className="border rounded-xl bg-card">
                    <Table>
                        <TableHeader className="bg-muted! rounded-t-xl overflow-hidden!">
                            <TableRow>
                                <TableHead scope="col">Employee</TableHead>
                                <TableHead scope="col">Transfer</TableHead>
                                <TableHead scope="col" className="text-right">Decision</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody className="[&_td]:py-4 [&_td]:text-[#023459]">
                            {items.map(t => (
                                <Fragment key={t.id}>
                                    <TableRow>
                                        <TableCell className="font-medium">{t.employeeName}</TableCell>
                                        <TableCell>
                                            <div className="flex flex-col gap-1">
                                                <span className="font-medium">
                                                    {t.days?.toLocaleString("da-DK")} days from weeks 1–5 earned {t.fromEarningYear}
                                                </span>
                                                <span className="text-xs text-muted-foreground">
                                                    Moved to the next holiday year · submitted {formatDate(t.createdAt!)}
                                                </span>
                                                {t.comment && <span className="text-sm italic text-[#516080] whitespace-normal">“{t.comment}”</span>}
                                            </div>
                                        </TableCell>
                                        <TableCell className="text-right">
                                            <div className="flex justify-end gap-2">
                                                <button type="button" disabled={busyId === t.id} className={primaryButton}
                                                        aria-label={`Approve transfer from ${t.employeeName}`}
                                                        onClick={() => decide(t.id!, () => api.api.transfersApprove({id: t.id!}))}>
                                                    <Check className="size-4"/> Approve
                                                </button>
                                                <button type="button" disabled={busyId === t.id} className={secondaryButton}
                                                        aria-label={`Reject transfer from ${t.employeeName}`}
                                                        onClick={() => { setRejectingId(rejectingId === t.id ? null : t.id!); setReason(""); }}>
                                                    <X className="size-4"/> Reject
                                                </button>
                                            </div>
                                        </TableCell>
                                    </TableRow>
                                    {rejectingId === t.id && (
                                        <TableRow className="bg-muted/40 hover:bg-muted/40">
                                            <TableCell colSpan={3}>
                                                <div className="flex flex-col gap-2">
                                                    <label htmlFor={`transfer-reason-${t.id}`} className="text-sm font-medium text-[#023459]">Reason for rejecting</label>
                                                    <textarea id={`transfer-reason-${t.id}`} value={reason} maxLength={500} rows={3}
                                                              className="w-full rounded-lg border-2 border-[#E9E1D6] bg-card p-3 text-sm text-[#023459] outline-none focus-visible:border-[#CE6B4C]"
                                                              onChange={e => setReason(e.target.value)}/>
                                                    <div className="flex gap-2">
                                                        <button type="button" disabled={busyId === t.id || !reason.trim()} className={primaryButton}
                                                                onClick={() => decide(t.id!, () => api.api.transfersReject({id: t.id!}, {reason: reason.trim()}))}>
                                                            Reject transfer
                                                        </button>
                                                        <button type="button" className={secondaryButton} onClick={() => setRejectingId(null)}>
                                                            Keep pending
                                                        </button>
                                                    </div>
                                                </div>
                                            </TableCell>
                                        </TableRow>
                                    )}
                                </Fragment>
                            ))}
                        </TableBody>
                    </Table>
                </div>
            )}
        </section>
    );
}