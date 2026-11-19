import {useEffect, useState} from "react";
import {api} from "@/api/client.ts";
import {type RequestDto} from "@/api/Api.ts";
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
};

export function MyRequestsView() {
    const {user} = useAuth();
    const [requests, setRequests] = useState<RequestDto[]>([]);
    const [busyId, setBusyId] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);
    const isAccounting = user?.role === "Accountant" || user?.role === "Admin";

    const load = () => {
        if (!user?.id) return;
        // /employees/{id}/requests starter ikke med /api, så metoden ligger under api.employees
        api.employees.requestsGetForEmployee({employeeId: user.id})
            .then(setRequests)
            .catch(() => setError("Could not load your requests."));
    };

    useEffect(load, [user?.id]);

    const handleCancel = async (id: string) => {
        setBusyId(id);
        setError(null);
        try {
            await api.api.approvalsCancel({id});
            load();
        } catch (err: any) {
            const problem = await err?.json?.().catch(() => null);
            setError(problem?.detail ?? "Could not cancel the request.");
        } finally {
            setBusyId(null);
        }
    };

    // Godkendt ferie, der ikke er startet endnu – nærmeste først
    const upcoming = requests
        .filter(r => r.status === "Approved" && r.startDate! >= today())
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
                        <p className="text-3xl font-semibold text-[#023459]">{requests.filter(r => r.status === "Pending").length}</p>
                        <p className="text-xs text-muted-foreground">The hours are reserved until accounting decides</p>
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
                            const started = r.startDate! < today();
                            const canCancel = r.status === "Pending" || (r.status === "Approved" && !started && isAccounting);
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
                                            <Badge className={statusStyles[r.status!]}>{r.status}</Badge>
                                            {r.rejectionReason && <span className="text-xs text-[#B5583B] whitespace-normal">“{r.rejectionReason}”</span>}
                                        </div>
                                    </TableCell>
                                    <TableCell>{formatDate(r.createdAt!)}</TableCell>
                                    <TableCell className="text-right">
                                        {canCancel ? (
                                            <button type="button" disabled={busyId === r.id} onClick={() => handleCancel(r.id!)}
                                                    className="inline-flex h-9 items-center rounded-lg border border-[#E9E1D6] bg-card px-4 text-sm font-medium text-[#023459] hover:bg-muted disabled:opacity-50">
                                                {r.status === "Pending" ? "Withdraw" : "Cancel"}
                                            </button>
                                        ) : r.status === "Approved" && !started ? (
                                            <span className="text-xs text-muted-foreground">Ask accounting to cancel</span>
                                        ) : null}
                                    </TableCell>
                                </TableRow>
                            );
                        })}
                    </TableBody>
                </Table>
            </div>
        </div>
    );
}