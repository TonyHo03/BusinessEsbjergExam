import {useEffect, useState} from "react";
import {AlertTriangle, CalendarPlus} from "lucide-react";
import {useNavigate} from "react-router";
import {api} from "@/api/client.ts";
import {type BalancesResponse, type TeamCalendarResponse} from "@/api/Api.ts";
import {useAuth} from "@/components/authProvider.tsx";
import {routes} from "@/components/app-sidebar.tsx";
import {formatDate, formatDateRange} from "@/lib/format.ts";
import {Label} from "@/components/ui/label.tsx";
import {Separator} from "@/components/ui/separator.tsx";
import {Input} from "@/components/ui/input.tsx";
import {Badge} from "@/components/ui/badge.tsx";
import {Card, CardContent} from "@/components/ui/card.tsx";
import {Table, TableBody, TableCell, TableHead, TableHeader, TableRow} from "@/components/ui/table.tsx";
import {TransferDialog} from "../components/TransferDialog.tsx";

const toIso = (d: Date) => d.toLocaleDateString("sv-SE"); // YYYY-MM-DD i lokal tid
const today = () => toIso(new Date());
const addDays = (iso: string, days: number) => { const d = new Date(iso); d.setDate(d.getDate() + days); return toIso(d); };
const hours = (h?: number | null) => h == null ? "–" : `${h.toLocaleString("da-DK", {minimumFractionDigits: 2, maximumFractionDigits: 2})} h`;
const days = (h?: number | null, dayLength?: number | null) =>
    h == null || !dayLength ? "" : `${(h / dayLength).toLocaleString("da-DK", {maximumFractionDigits: 1})} days`;

// Uge 1–5 optjenes 1.9–31.8, 6. ferieuge i kalenderåret
const periodLabel = (account?: string | null, year?: number | null) =>
    account === "Week6" ? `6th week · earned ${year}` : `Weeks 1–5 · earned 1 Sep ${year} – 31 Aug ${(year ?? 0) + 1}`;

const statusStyles: Record<string, string> = {
    Active: "bg-success-bg text-success-fg",
    ExpiringSoon: "bg-[#CE6B4C]/15 text-[#B5583B]",
    Expired: "bg-destructive-bg text-destructive-fg",
    NotYetUsable: "bg-muted text-muted-foreground",
};
const statusLabels: Record<string, string> = {
    Active: "Active", ExpiringSoon: "Expires soon", Expired: "Expired", NotYetUsable: "Not usable yet",
};

export function DashboardView() {
    const {user} = useAuth();
    const navigate = useNavigate();
    const [asOf, setAsOf] = useState<string>(today());
    const [balances, setBalances] = useState<BalancesResponse | null>(null);
    const [balanceError, setBalanceError] = useState<string | null>(null);
    const [team, setTeam] = useState<TeamCalendarResponse | null>(null);
    const [transferFrom, setTransferFrom] = useState<{year: number, days: number} | null>(null);
    const [notice, setNotice] = useState<string | null>(null);

    useEffect(() => {
        if (!user?.id) return;
        setBalanceError(null);
        api.api.balancesGet({employeeId: user.id, asOf})
            .then(setBalances)
            .catch(() => { setBalances(null); setBalanceError("Your balance can't be shown. You may not have a work schedule yet."); });
    }, [user?.id, asOf]);

    // Teamkalenderen viser 4 uger fra den valgte dato
    useEffect(() => {
        api.teamHolidays.teamHolidaysGet({from: asOf, to: addDays(asOf, 27)})
            .then(setTeam)
            .catch(() => setTeam(null));
    }, [asOf]);

    const needsAction = balances?.periods?.filter(p => p.needsAction) ?? [];
    const isToday = asOf === today();

    return (
        <div className="flex flex-col w-full h-full p-12 gap-4">
            <div className="flex w-full items-end gap-4">
                <div className="flex flex-col gap-4">
                    <Label className="text-[#0F2338] text-3xl font-normal">Hi, {user?.name?.split(" ")[0]}</Label>
                    <Label className="text-[#516080]">Your holiday balance and who on the team is away.</Label>
                </div>
                <div className="flex items-end gap-3 ml-auto">
                    <div className="flex flex-col gap-2">
                        <Label htmlFor="asOf">Balance on</Label>
                        <Input id="asOf" type="date" className="bg-card!" min={today()} value={asOf}
                               onChange={e => e.target.value && setAsOf(e.target.value)}/>
                    </div>
                    <button type="button" onClick={() => navigate(routes.requestHolidays)}
                            className="flex h-9 items-center gap-2 rounded-lg bg-primary px-6 text-sm font-medium text-primary-foreground">
                        <CalendarPlus className="size-4"/> Request holiday
                    </button>
                </div>
            </div>
            <Separator className="h-px"/>

            {balanceError && (
                <p role="alert" className="rounded-lg border border-[#CE6B4C] bg-[#CE6B4C]/10 px-4 py-3 text-sm text-[#B5583B]">{balanceError}</p>
            )}

            {notice && (
                <p role="status" className="rounded-lg border border-success-fg/30 bg-success-bg px-4 py-3 text-sm text-success-fg">{notice}</p>
            )}

            {needsAction.length > 0 && (
                <div role="status" className="flex items-start gap-3 rounded-lg border border-[#CE6B4C] bg-[#CE6B4C]/10 px-4 py-3 text-sm text-[#B5583B]">
                    <AlertTriangle className="size-5 shrink-0"/>
                    <div>
                        {needsAction.map(p => (
                            <div key={`${p.account}-${p.earningYear}`} className="flex items-center gap-3">
                                <span>
                                    <strong>{hours(p.remaining)}</strong> ({p.remainingDays?.toLocaleString("da-DK")} days) {p.status === "Expired" ? "expired" : "expire"} on {formatDate(p.expiresOn!)}.
                                </span>
                                {p.account === "Weeks1To5" && p.status === "ExpiringSoon" && (
                                    <button type="button" onClick={() => setTransferFrom({year: p.earningYear!, days: p.remainingDays ?? 0})}
                                            className="inline-flex h-8 items-center rounded-lg border border-[#CE6B4C] bg-card px-3 text-xs font-medium text-[#B5583B] hover:bg-[#CE6B4C]/10">
                                        Request transfer
                                    </button>
                                )}
                            </div>
                        ))}
                        <div>Take the holiday before then, or request a transfer of up to 5 days to the next holiday year.</div>
                    </div>
                </div>
            )}

            {balances && (
                <>
                    <div className="flex w-full gap-4">
                        <Card className="flex-1">
                            <CardContent className="flex flex-col gap-6">
                                <p className="text-sm text-muted-foreground">Weeks 1–5 {isToday ? "available now" : `available on ${formatDate(asOf)}`}</p>
                                <p className="text-3xl font-semibold text-[#023459]">{hours(balances.availableWeeks1To5)}</p>
                                <p className="text-xs text-muted-foreground">{days(balances.availableWeeks1To5, balances.dayLength)} · 1 day = {hours(balances.dayLength)}</p>
                            </CardContent>
                        </Card>
                        <Card className="flex-1">
                            <CardContent className="flex flex-col gap-6">
                                <p className="text-sm text-muted-foreground">6th week {isToday ? "available now" : `available on ${formatDate(asOf)}`}</p>
                                <p className="text-3xl font-semibold text-[#023459]">{hours(balances.availableWeek6)}</p>
                                <p className="text-xs text-muted-foreground">{days(balances.availableWeek6, balances.dayLength)}</p>
                            </CardContent>
                        </Card>
                        <Card className="flex-1">
                            <CardContent className="flex flex-col gap-6">
                                <p className="text-sm text-muted-foreground">Waiting for approval</p>
                                <p className="text-3xl font-semibold text-[#023459]">{hours((balances.pendingWeeks1To5 ?? 0) + (balances.pendingWeek6 ?? 0))}</p>
                                <p className="text-xs text-muted-foreground">Already subtracted from what's available</p>
                            </CardContent>
                        </Card>
                    </div>

                    <div className="border rounded-xl bg-card">
                        <Table>
                            <TableHeader className="bg-muted! rounded-t-xl overflow-hidden!">
                                <TableRow>
                                    <TableHead>Period</TableHead>
                                    <TableHead>Earned</TableHead>
                                    <TableHead>Used</TableHead>
                                    <TableHead>Remaining</TableHead>
                                    <TableHead>Can be used</TableHead>
                                    <TableHead>Status</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody className="[&_td]:py-4 [&_td]:text-[#023459]">
                                {(balances.periods?.length ?? 0) === 0 && (
                                    <TableRow>
                                        <TableCell colSpan={6} className="text-center text-muted-foreground!">
                                            No holiday earned yet. Holiday is earned at the end of each month you are employed.
                                        </TableCell>
                                    </TableRow>
                                )}
                                {balances.periods?.map(p => (
                                    <TableRow key={`${p.account}-${p.earningYear}`}>
                                        <TableCell className="font-medium">{periodLabel(p.account, p.earningYear)}</TableCell>
                                        <TableCell>{hours(p.accrued)}</TableCell>
                                        <TableCell>{hours(p.used)}</TableCell>
                                        <TableCell>
                                            <div className="flex flex-col">
                                                <span className="font-semibold">{hours(p.remaining)}</span>
                                                <span className="text-xs text-muted-foreground">{p.remainingDays?.toLocaleString("da-DK")} days</span>
                                            </div>
                                        </TableCell>
                                        <TableCell>{formatDateRange(p.usableFrom!, p.expiresOn!)}</TableCell>
                                        <TableCell><Badge className={statusStyles[p.status!]}>{statusLabels[p.status!]}</Badge></TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    </div>
                </>
            )}

            <TeamCalendar team={team} from={asOf}/>

            {user?.id && (
                <TransferDialog employeeId={user.id} earningYear={transferFrom?.year ?? null} remainingDays={transferFrom?.days ?? 0}
                                onClose={() => setTransferFrom(null)}
                                onSent={() => setNotice("Your transfer request has been sent to accounting. You can follow it under My requests.")}/>
            )}
        </div>
    );
}

const WEEKDAYS = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
const MONTHS = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

function TeamCalendar({team, from}: {team: TeamCalendarResponse | null, from: string}) {
    if (!team) return null;

    const workingDays = (team.days ?? []).filter(d => d.isWorkingDay);
    const members = team.members ?? [];
    const isAway = (absences: {startDate?: string, endDate?: string}[] | null | undefined, date: string) =>
        (absences ?? []).some(a => a.startDate! <= date && date <= a.endDate!);
    // Ny uge starter mandag – den dag får en tydeligere kant til venstre
    const startsWeek = (date: string) => new Date(date).getDay() === 1;
    const anyoneAway = workingDays.some(d => (d.absent ?? 0) > 0);
    const fromLabel = from === today() ? "today" : formatDate(from);

    return (
        <div className="flex flex-col gap-4 mt-4">
            <div className="flex items-end">
                <div className="flex flex-col gap-1">
                    <Label className="text-[#0F2338] text-xl font-normal">Team holiday – 4 weeks from {fromLabel}</Label>
                    <Label className="text-[#516080]">Approved holiday only. Weekends and public holidays are left out.</Label>
                </div>
                <div className="flex items-center gap-4 ml-auto text-xs text-[#516080]">
                    <span className="flex items-center gap-2"><span className="size-3 rounded-sm bg-[#CE6B4C]"/> On holiday</span>
                    <span className="flex items-center gap-2"><span className="size-3 rounded-sm bg-muted"/> Working</span>
                </div>
            </div>

            <div className="rounded-xl border bg-card p-4 overflow-x-auto">
                <table className="w-full border-separate border-spacing-0 text-xs text-[#023459]">
                    <thead>
                    <tr>
                        <th className="sticky left-0 bg-card w-40 pr-3 text-left font-medium text-muted-foreground">Colleague</th>
                        {workingDays.map(d => {
                            const date = new Date(d.date!);
                            const firstOfMonth = date.getDate() === 1 || d === workingDays[0];
                            return (
                                <th key={d.date} scope="col"
                                    className={`min-w-7 px-0.5 pb-2 font-normal text-center ${startsWeek(d.date!) ? "border-l-2 border-[#E9E1D6]" : ""}`}>
                                    <div className="text-[10px] text-muted-foreground">{WEEKDAYS[date.getDay()]![0]}</div>
                                    <div className="font-medium">{date.getDate()}</div>
                                    <div className="h-3 text-[10px] text-muted-foreground">{firstOfMonth ? MONTHS[date.getMonth()] : ""}</div>
                                </th>
                            );
                        })}
                    </tr>
                    </thead>
                    <tbody>
                    {members.map(m => (
                        <tr key={m.employeeId}>
                            <th scope="row" className="sticky left-0 bg-card pr-3 py-1 text-left font-medium whitespace-nowrap">{m.name}</th>
                            {workingDays.map(d => {
                                const away = isAway(m.absences, d.date!);
                                return (
                                    <td key={d.date} className={`px-0.5 py-1 ${startsWeek(d.date!) ? "border-l-2 border-[#E9E1D6]" : ""}`}>
                                        <div className={`h-6 rounded-sm ${away ? "bg-[#CE6B4C]" : "bg-muted"}`}
                                             title={`${m.name} · ${formatDate(d.date!)}${away ? " · on holiday" : ""}`}
                                             aria-label={away ? `${m.name} on holiday ${formatDate(d.date!)}` : undefined}/>
                                    </td>
                                );
                            })}
                        </tr>
                    ))}
                    <tr>
                        <th scope="row" className="sticky left-0 bg-card pr-3 pt-3 text-left font-medium text-muted-foreground">Away</th>
                        {workingDays.map(d => (
                            <td key={d.date} className={`pt-3 text-center ${startsWeek(d.date!) ? "border-l-2 border-[#E9E1D6]" : ""}`}>
                                    <span className={(d.absent ?? 0) > 0 ? "font-semibold text-[#B5583B]" : "text-muted-foreground"}>
                                        {d.absent ?? 0}
                                    </span>
                            </td>
                        ))}
                    </tr>
                    </tbody>
                </table>

                {!anyoneAway && (
                    <p className="mt-3 text-sm text-muted-foreground">Nobody on the team has approved holiday in these 4 weeks.</p>
                )}
            </div>
        </div>
    );
}