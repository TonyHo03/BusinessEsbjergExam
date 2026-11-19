import {useEffect, useState} from "react";
import {AlertTriangle} from "lucide-react";
import {api} from "@/api/client.ts";
import {type EmployeeOverviewRow} from "@/api/Api.ts";
import {Label} from "@/components/ui/label.tsx";
import {Separator} from "@/components/ui/separator.tsx";
import {Input} from "@/components/ui/input.tsx";
import {Badge} from "@/components/ui/badge.tsx";
import {Card, CardContent} from "@/components/ui/card.tsx";
import {Avatar, AvatarFallback} from "@/components/ui/avatar.tsx";
import {Table, TableBody, TableCell, TableHead, TableHeader, TableRow} from "@/components/ui/table.tsx";
import {initials} from "@/components/nav-user.tsx";

const today = () => new Date().toLocaleDateString("sv-SE"); // YYYY-MM-DD i lokal tid
const formatDate = (iso?: string | null) => iso ? new Date(iso).toLocaleDateString("da-DK") : "–";
const hours = (h?: number | null) => h == null ? "–" : `${h.toLocaleString("da-DK", {minimumFractionDigits: 2, maximumFractionDigits: 2})} h`;
const days = (h?: number | null, dayLength?: number | null) =>
    h == null || !dayLength ? null : `${(h / dayLength).toLocaleString("da-DK", {maximumFractionDigits: 1})} days`;

export function EmployeeOverviewView() {
    const [rows, setRows] = useState<EmployeeOverviewRow[]>([]);
    const [asOf, setAsOf] = useState<string>(today());
    const [search, setSearch] = useState<string>("");
    const [onlyExpiring, setOnlyExpiring] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        setError(null);
        api.api.employeeOverviewGet({asOf})
            .then(setRows)
            .catch(() => setError("Could not load the employees."));
    }, [asOf]);

    const expiring = rows.filter(r => (r.expiringHours ?? 0) > 0);
    const term = search.trim().toLowerCase();
    const filtered = rows.filter(r =>
        (!term || r.name?.toLowerCase().includes(term) || r.email?.toLowerCase().includes(term)) &&
        (!onlyExpiring || (r.expiringHours ?? 0) > 0));

    return (
        <div className="flex flex-col w-full h-full p-12 gap-4">
            <div className="flex w-full items-end gap-4">
                <div className="flex flex-col gap-4">
                    <Label className="text-[#0F2338] text-3xl font-normal">Employee overview</Label>
                    <Label className="text-[#516080]">Balances, pending requests and holiday that needs action for every employee.</Label>
                </div>
                <div className="flex flex-col gap-2 ml-auto">
                    <Label htmlFor="asOf">Balances as of</Label>
                    <Input id="asOf" type="date" className="bg-card!" value={asOf} onChange={e => e.target.value && setAsOf(e.target.value)}/>
                </div>
            </div>
            <Separator className="h-px"/>

            {error && (
                <p role="alert" className="rounded-lg border border-[#CE6B4C] bg-[#CE6B4C]/10 px-4 py-3 text-sm text-[#B5583B]">{error}</p>
            )}

            <div className="flex w-full gap-4">
                <Card className="flex-1">
                    <CardContent className="flex flex-col gap-6">
                        <p className="text-sm text-muted-foreground">Employees</p>
                        <p className="text-3xl font-semibold text-[#023459]">{rows.length}</p>
                        <p className="text-xs text-muted-foreground">{rows.filter(r => !r.hasSchedule).length} without a work schedule</p>
                    </CardContent>
                </Card>
                <Card className="flex-1">
                    <CardContent className="flex flex-col gap-6">
                        <p className="text-sm text-muted-foreground">Holiday that needs action</p>
                        <p className="text-3xl font-semibold text-[#B5583B]">{expiring.length}</p>
                        <p className="text-xs text-muted-foreground">
                            {hours(expiring.reduce((sum, r) => sum + (r.expiringHours ?? 0), 0))} expiring or expired – must be taken, transferred or paid out
                        </p>
                    </CardContent>
                </Card>
                <Card className="flex-1">
                    <CardContent className="flex flex-col gap-6">
                        <p className="text-sm text-muted-foreground">Pending requests</p>
                        <p className="text-3xl font-semibold text-[#023459]">{hours(rows.reduce((sum, r) => sum + (r.pendingHours ?? 0), 0))}</p>
                        <p className="text-xs text-muted-foreground">Reserved until accounting decides</p>
                    </CardContent>
                </Card>
            </div>

            <Separator className="h-px"/>
            <div className="flex w-full items-end">
                <div className="flex flex-col gap-2">
                    <Label htmlFor="search">Find an employee:</Label>
                    <Input id="search" className="bg-card! w-72" placeholder="Search by name or email..."
                           value={search} onChange={e => setSearch(e.target.value)}/>
                </div>
                <label className="flex items-center gap-2 ml-auto text-sm text-[#023459]">
                    <input type="checkbox" checked={onlyExpiring} onChange={e => setOnlyExpiring(e.target.checked)}
                           className="size-4 accent-[#CE6B4C]"/>
                    Only show holiday that needs action
                </label>
            </div>

            <div className="border rounded-xl bg-card">
                <Table>
                    <TableHeader className="bg-muted! rounded-t-xl overflow-hidden!">
                        <TableRow>
                            <TableHead>Employee</TableHead>
                            <TableHead>Weeks 1–5</TableHead>
                            <TableHead>6th week</TableHead>
                            <TableHead>Pending</TableHead>
                            <TableHead>Needs action</TableHead>
                            <TableHead>Status</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody className="[&_td]:py-4 [&_td]:text-[#023459]">
                        {filtered.length === 0 && (
                            <TableRow>
                                <TableCell colSpan={6} className="text-center text-muted-foreground">No employees match your search.</TableCell>
                            </TableRow>
                        )}
                        {filtered.map(r => (
                            <TableRow key={r.employeeId}>
                                <TableCell>
                                    <div className="flex items-center gap-3">
                                        <Avatar>
                                            <AvatarFallback className="bg-muted text-xs font-medium text-[#023459]">{initials(r.name)}</AvatarFallback>
                                        </Avatar>
                                        <div className="flex flex-col">
                                            <span className="font-medium">{r.name}</span>
                                            <span className="text-xs text-muted-foreground">{r.email}</span>
                                        </div>
                                    </div>
                                </TableCell>
                                {r.hasSchedule ? (
                                    <>
                                        <TableCell>
                                            <div className="flex flex-col">
                                                <span className="font-semibold">{hours(r.availableWeeks1To5)}</span>
                                                <span className="text-xs text-muted-foreground">{days(r.availableWeeks1To5, r.dayLength)}</span>
                                            </div>
                                        </TableCell>
                                        <TableCell>{hours(r.availableWeek6)}</TableCell>
                                    </>
                                ) : (
                                    <TableCell colSpan={2} className="text-muted-foreground">No work schedule</TableCell>
                                )}
                                <TableCell>{(r.pendingHours ?? 0) > 0 ? hours(r.pendingHours) : <span className="text-muted-foreground">–</span>}</TableCell>
                                <TableCell>
                                    {(r.expiringHours ?? 0) > 0 ? (
                                        <Badge className="bg-[#CE6B4C]/15 text-[#B5583B] gap-1">
                                            <AlertTriangle className="size-3"/>
                                            {hours(r.expiringHours)} · {r.expiresOn && r.expiresOn < asOf ? "expired" : "expires"} {formatDate(r.expiresOn)}
                                        </Badge>
                                    ) : <span className="text-muted-foreground">–</span>}
                                </TableCell>
                                <TableCell>
                                    <Badge className={r.status === "Active" ? "bg-success-bg text-success-fg" : "bg-destructive-bg text-destructive-fg"}>
                                        {r.status}
                                    </Badge>
                                </TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </div>
        </div>
    );
}