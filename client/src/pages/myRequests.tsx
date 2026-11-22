import {useEffect, useState} from "react";
import {api} from "@/api/client.ts";
import {type RequestDto, type TransferDto} from "@/api/Api.ts";
import {useAuth} from "@/components/authProvider.tsx";
import {accountLabel, formatDate, formatDateRange, formatHours} from "@/lib/format.ts";
import {Label} from "@/components/ui/label.tsx";
import {Separator} from "@/components/ui/separator.tsx";
import {Badge} from "@/components/ui/badge.tsx";
import {Card, CardContent} from "@/components/ui/card.tsx";
import {Table, TableBody, TableCell, TableHead, TableHeader, TableRow} from "@/components/ui/table.tsx";

const today = () => new Date().toLocaleDateString("sv-SE"); // YYYY-MM-DD i lokal tid

const statusStyles: Record<string, string> = {
    Pending: "bg-[#FEF3C7] text-[#92400E]",
    Approved: "bg-success-bg text-success-fg",
    Rejected: "bg-destructive-bg text-destructive-fg",
    Cancelled: "bg-muted text-muted-foreground",
    CancellationRequested: "bg-[#FEF3C7] text-[#92400E]",
};
const statusLabels: Record<string, string> = {
    Pending: "Pending", Approved: "Approved", Rejected: "Rejected", Cancelled: "Cancelled",
    CancellationRequested: "Cancellation requested",
};

const buttonStyle =
    "inline-flex h-9 items-center rounded-lg border border-[#E9E1D6] bg-card px-4 text-sm font-medium text-[#023459] hover:bg-muted disabled:opacity-50";

export function MyRequestsView() {
    const {user} = useAuth();
    const [requests, setRequests] = useState<RequestDto[]>([]);
    const [transfers, setTransfers] = useState<TransferDto[]>([]);
    const [busyId, setBusyId] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);
    const isAccounting = user?.role === "Accountant" || user?.role === "Admin";

    const load = () => {
        if (!user?.id) return;
        // /employees/{id}/requests starter ikke med /api, så metoden ligger under api.employees
        api.employees.requestsGetForEmployee({employeeId: user.id})
            .then(setRequests)
            .catch(() => setError("Could not load your requests."));
        api.employees.transfersGetForEmployee({employeeId: user.id})
            .then(setTransfers)
            .catch(() => setTransfers([]));
    };

    useEffect(load, [user?.id]);

    const run = async (id: string, action: () => Promise<unknown>, fallback: string) => {
        setBusyId(id);
        setError(null);
        try {
            await action();
            load();
        } catch (err: any) {
            const problem = await err?.json?.().catch(() => null);
            setError(problem?.detail ?? fallback);
        } finally {
            setBusyId(null);
        }
    };

    const handleCancel = (id: string) =>
        run(id, () => api.api.approvalsCancel({id}), "Could not cancel the request.");

    const handleRequestCancellation = (id: string) =>
        run(id, () => api.api.approvalsRequestCancellation({id}), "Could not ask for a cancellation.");

    // Godkendt ferie, der ikke er startet endnu – nærmeste først
    const upcoming = requests
        .filter(r => (r.status === "Approved" || r.status === "CancellationRequested") && r.startDate! >= today())
        .sort((a, b) => a.startDate!.localeCompare(b.startDate!));

    return (
        <div className="flex flex-col w-full h-full p-12 gap-4">
            <Label className="text-[#0F2338] text-3xl font-normal">My requests</Label>
            <Label className="text-[#516080]">All your holiday requests and what accounting decided.</Label>
            <Separator className="h-px"/>

            {error && (
                <p role="alert" className="rounded-lg border border-[#CE6B4C] bg-[#CE6B4C]/10 px-4 py-3 text-sm text-[#B5583B]">{error}</p>
            )}

            <div className="flex w-full gap-4">
                <Card className="flex-1">
                    <CardContent className="flex flex-col gap-6">
                        <p className="text-sm text-muted-foreground">Waiting for a decision</p>
                        <p className="text-3xl font-semibold text-[#023459]">
                            {requests.filter(r => r.status === "Pending" || r.status === "CancellationRequested").length}
                        </p>
                        <p className="text-xs text-muted-foreground">Requests and cancellations accounting hasn't decided yet</p>
                    </CardContent>
                </Card>
                <Card className="flex-1">
                    <CardContent className="flex flex-col gap-6">
                        <p className="text-sm text-muted-foreground">Upcoming holiday</p>
                        <p className="text-3xl font-semibold text-[#023459]">{upcoming.length}</p>
                        <p className="text-xs text-muted-foreground">
                            {upcoming.length > 0 ? `Next: ${formatDateRange(upcoming[0].startDate!, upcoming[0].endDate!)}` : "No approved holiday ahead"}
                        </p>
                    </CardContent>
                </Card>
            </div>

            <div className="border rounded-xl bg-card">
                <Table>
                    <TableHeader className="bg-muted! rounded-t-xl overflow-hidden!">
                        <TableRow>
                            <TableHead>Period</TableHead>
                            <TableHead>Account</TableHead>
                            <TableHead>Hours</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead>Submitted</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody className="[&_td]:py-4 [&_td]:text-[#023459]">
                        {requests.length === 0 && (
                            <TableRow>
                                <TableCell colSpan={6} className="text-center text-muted-foreground">You have no holiday requests yet.</TableCell>
                            </TableRow>
                        )}
                        {requests.map(r => {
                            const started = r.startDate! <= today();
                            const busy = busyId === r.id;
                            return (
                                <TableRow key={r.id}>
                                    <TableCell>
                                        <div className="flex flex-col">
                                            <span className="font-medium">{formatDateRange(r.startDate!, r.endDate!)}</span>
                                            {r.comment && <span className="text-xs text-muted-foreground">{r.comment}</span>}
                                        </div>
                                    </TableCell>
                                    <TableCell>{accountLabel(r.account!)}</TableCell>
                                    <TableCell>{formatHours(r.hours!)}</TableCell>
                                    <TableCell>
                                        <div className="flex flex-col gap-1 items-start">
                                            <Badge className={statusStyles[r.status!]}>{statusLabels[r.status!] ?? r.status}</Badge>
                                            {r.rejectionReason && (
                                                <span className="text-xs text-[#B5583B] whitespace-normal">
                                                    {r.status === "Approved" ? "Cancellation declined: " : ""}“{r.rejectionReason}”
                                                </span>
                                            )}
                                        </div>
                                    </TableCell>
                                    <TableCell>{formatDate(r.createdAt!)}</TableCell>
                                    <TableCell className="text-right">
                                        {r.status === "Pending" ? (
                                            <button type="button" disabled={busy} onClick={() => handleCancel(r.id!)} className={buttonStyle}>
                                                Withdraw
                                            </button>
                                        ) : r.status === "Approved" && !started && isAccounting ? (
                                            <button type="button" disabled={busy} onClick={() => handleCancel(r.id!)} className={buttonStyle}>
                                                Cancel
                                            </button>
                                        ) : r.status === "Approved" && !started ? (
                                            <button type="button" disabled={busy} onClick={() => handleRequestCancellation(r.id!)} className={buttonStyle}>
                                                Request cancellation
                                            </button>
                                        ) : r.status === "CancellationRequested" ? (
                                            <span className="text-xs text-muted-foreground">Waiting for accounting</span>
                                        ) : null}
                                    </TableCell>
                                </TableRow>
                            );
                        })}
                    </TableBody>
                </Table>
            </div>

            {transfers.length > 0 && (
                <div className="flex flex-col gap-4 mt-4">
                    <Label className="text-[#0F2338] text-xl font-normal">Transfers to the next holiday year</Label>
                    <div className="border rounded-xl bg-card">
                        <Table>
                            <TableHeader className="bg-muted! rounded-t-xl overflow-hidden!">
                                <TableRow>
                                    <TableHead>From period</TableHead>
                                    <TableHead>Days</TableHead>
                                    <TableHead>Status</TableHead>
                                    <TableHead>Submitted</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody className="[&_td]:py-4 [&_td]:text-[#023459]">
                                {transfers.map(t => (
                                    <TableRow key={t.id}>
                                        <TableCell>
                                            <div className="flex flex-col">
                                                <span className="font-medium">Weeks 1–5 earned {t.fromEarningYear}</span>
                                                {t.comment && <span className="text-xs text-muted-foreground">{t.comment}</span>}
                                            </div>
                                        </TableCell>
                                        <TableCell>{t.days?.toLocaleString("da-DK")} days</TableCell>
                                        <TableCell>
                                            <div className="flex flex-col gap-1 items-start">
                                                <Badge className={statusStyles[t.status!]}>{statusLabels[t.status!] ?? t.status}</Badge>
                                                {t.rejectionReason && <span className="text-xs text-[#B5583B] whitespace-normal">“{t.rejectionReason}”</span>}
                                            </div>
                                        </TableCell>
                                        <TableCell>{formatDate(t.createdAt!)}</TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    </div>
                </div>
            )}
        </div>
    );
}