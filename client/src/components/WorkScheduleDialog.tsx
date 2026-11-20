import {useEffect, useState} from "react";
import * as React from "react";
import {api} from "@/api/client.ts";
import {type DayTimes, type EmployeeDto, type WorkScheduleDto} from "@/api/Api.ts";
import {Label} from "@/components/ui/label.tsx";
import {Input} from "@/components/ui/input.tsx";
import {Button} from "@/components/ui/button.tsx";
import {Badge} from "@/components/ui/badge.tsx";
import {Trash2} from "lucide-react";
import {
    Dialog,
    DialogClose,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle
} from "@/components/ui/dialog.tsx";

type DayKey = "monday" | "tuesday" | "wednesday" | "thursday" | "friday";
type DayForm = { working: boolean, start: string, end: string };

const DAYS: { key: DayKey, label: string }[] = [
    {key: "monday", label: "Monday"},
    {key: "tuesday", label: "Tuesday"},
    {key: "wednesday", label: "Wednesday"},
    {key: "thursday", label: "Thursday"},
    {key: "friday", label: "Friday"},
];

// Business Esbjergs standard: man–tor 08:00–15:30, fre 08:00–15:00 (37 t)
const STANDARD: Record<DayKey, DayForm> = {
    monday: {working: true, start: "08:00", end: "15:30"},
    tuesday: {working: true, start: "08:00", end: "15:30"},
    wednesday: {working: true, start: "08:00", end: "15:30"},
    thursday: {working: true, start: "08:00", end: "15:30"},
    friday: {working: true, start: "08:00", end: "15:00"},
};

const today = () => new Date().toLocaleDateString("sv-SE"); // YYYY-MM-DD i lokal tid
const formatDate = (iso?: string | null) => iso ? new Date(iso).toLocaleDateString("da-DK") : "";
const formatHours = (h: number) => `${h.toLocaleString("da-DK", {maximumFractionDigits: 2})} h`;

const toMinutes = (time: string) => { const [h, m] = time.split(":").map(Number); return (h ?? 0) * 60 + (m ?? 0); };
const dayHours = (d: DayForm) => d.working && d.start && d.end && toMinutes(d.end) > toMinutes(d.start)
    ? (toMinutes(d.end) - toMinutes(d.start)) / 60 : 0;

const fromDto = (d?: DayTimes | null): DayForm =>
    d?.start && d?.end ? {working: true, start: d.start, end: d.end} : {working: false, start: "08:00", end: "15:30"};

const describeDay = (d?: DayTimes | null) => d?.start && d?.end ? `${d.start}–${d.end}` : "Off";

export function WorkScheduleDialog({employee, onClose}: { employee: EmployeeDto | null, onClose: () => void }) {
    const [schedules, setSchedules] = useState<WorkScheduleDto[]>([]);
    const [startDate, setStartDate] = useState<string>("");
    const [days, setDays] = useState<Record<DayKey, DayForm>>(STANDARD);
    const [effectiveFrom, setEffectiveFrom] = useState<string>(today());
    const [errors, setErrors] = useState<string[]>([]);
    const [submitting, setSubmitting] = useState(false);
    const [confirmDelete, setConfirmDelete] = useState<string | null>(null);

    const load = async (employeeId: string) => {
        const response = await api.api.workSchedulesGet({employeeId});
        const list = response.schedules ?? [];
        setSchedules(list);
        setStartDate(response.employeeStartDate ?? "");

        // Første skema starter på ansættelsesdatoen, ellers tidligst i dag
        setEffectiveFrom(list.length === 0 ? (response.employeeStartDate ?? today()) : today());

        // Foreslå det nyeste skema, så man kun skal rette det, der ændrer sig
        const latest = list[0];
        setDays(latest ? {
            monday: fromDto(latest.monday), tuesday: fromDto(latest.tuesday), wednesday: fromDto(latest.wednesday),
            thursday: fromDto(latest.thursday), friday: fromDto(latest.friday),
        } : STANDARD);
    };

    useEffect(() => {
        if (!employee?.id) return;
        setErrors([]);
        setConfirmDelete(null);
        load(employee.id).catch(() => setErrors(["Could not load the work schedule."]));
    }, [employee?.id]);

    const setDay = (key: DayKey, change: Partial<DayForm>) => setDays(d => ({...d, [key]: {...d[key], ...change}}));
    const weeklyHours = DAYS.reduce((sum, d) => sum + dayHours(days[d.key]), 0);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!employee?.id) return;
        setSubmitting(true);
        setErrors([]);
        const toTimes = (d: DayForm): DayTimes => d.working ? {start: d.start, end: d.end} : {start: null, end: null};
        try {
            await api.api.workSchedulesAdd({employeeId: employee.id}, {
                effectiveFrom,
                monday: toTimes(days.monday), tuesday: toTimes(days.tuesday), wednesday: toTimes(days.wednesday),
                thursday: toTimes(days.thursday), friday: toTimes(days.friday),
            });
            await load(employee.id);
        } catch (err: any) {
            const body = await err?.json?.().catch(() => null);
            setErrors(Array.isArray(body) ? body : ["Could not save the work schedule."]);
        } finally {
            setSubmitting(false);
        }
    };

    // Kun det nyeste skema kan slettes, og kun hvis det ikke er startet endnu
    const handleDelete = async (schedule: WorkScheduleDto) => {
        if (!employee?.id || !schedule.id) return;
        setSubmitting(true);
        setErrors([]);
        try {
            await api.api.workSchedulesDelete({employeeId: employee.id, id: schedule.id});
            await load(employee.id);
        } catch (err: any) {
            const body = await err?.json?.().catch(() => null);
            setErrors([typeof body === "string" ? body : "Could not delete the work schedule."]);
        } finally {
            setSubmitting(false);
            setConfirmDelete(null);
        }
    };

    const hasSchedule = schedules.length > 0;
    const canDelete = (s: WorkScheduleDto) => s === schedules[0] && s.effectiveFrom! >= today();

    return (
        <Dialog open={employee !== null} onOpenChange={open => !open && onClose()}>
            <DialogContent className="sm:max-w-xl border-2 max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>Work schedule – {employee?.name}</DialogTitle>
                    <DialogDescription>
                        Hours per day decide how many hours a holiday day is worth. A new schedule applies from its start date – earlier dates keep the old one.
                    </DialogDescription>
                </DialogHeader>

                {hasSchedule ? (
                    <div className="flex flex-col gap-2 text-sm text-[#023459]">
                        <span className="font-medium">History</span>
                        {schedules.map(s => (
                            <div key={s.id} className="flex flex-col gap-1 rounded-lg border p-3">
                                <div className="flex items-center gap-2">
                                    <span className="font-semibold">
                                        From {formatDate(s.effectiveFrom)}{s.effectiveTo ? ` to ${formatDate(s.effectiveTo)}` : ""}
                                    </span>
                                    <span className="text-muted-foreground">· {formatHours(s.weeklyHours ?? 0)} per week</span>
                                    {s.isCurrent && <Badge className="bg-success-bg text-success-fg ml-auto">Current</Badge>}
                                    {!s.isCurrent && s.effectiveFrom! > today() && <Badge className="bg-[#CE6B4C]/15 text-[#B5583B] ml-auto">Upcoming</Badge>}
                                    {canDelete(s) && (confirmDelete === s.id ? (
                                        <div className="flex gap-1">
                                            <Button type="button" variant="destructive" size="sm" disabled={submitting}
                                                    onClick={() => handleDelete(s)}>Delete</Button>
                                            <Button type="button" variant="outline" size="sm" disabled={submitting}
                                                    onClick={() => setConfirmDelete(null)}>Cancel</Button>
                                        </div>
                                    ) : (
                                        <Button type="button" variant="ghost" size="icon" disabled={submitting}
                                                aria-label={`Delete schedule from ${formatDate(s.effectiveFrom)}`}
                                                onClick={() => setConfirmDelete(s.id!)}>
                                            <Trash2 className="size-4 text-destructive"/>
                                        </Button>
                                    ))}
                                </div>
                                <span className="text-xs text-muted-foreground">
                                    Mon {describeDay(s.monday)} · Tue {describeDay(s.tuesday)} · Wed {describeDay(s.wednesday)} · Thu {describeDay(s.thursday)} · Fri {describeDay(s.friday)}
                                </span>
                            </div>
                        ))}
                    </div>
                ) : (
                    <p className="rounded-lg border border-[#CE6B4C] bg-[#CE6B4C]/10 px-4 py-3 text-sm text-[#B5583B]">
                        {employee?.name} has no work schedule yet, so no balance can be calculated. Add one below.
                    </p>
                )}

                <form onSubmit={handleSubmit} className="grid gap-4">
                    <span className="text-sm font-medium text-[#023459]">{hasSchedule ? "New schedule" : "First schedule"}</span>

                    <div className="grid gap-2">
                        {DAYS.map(d => (
                            <div key={d.key} className="flex items-center gap-3 text-sm">
                                <label className="flex w-32 items-center gap-2">
                                    <input type="checkbox" className="size-4 accent-[#CE6B4C]" checked={days[d.key].working}
                                           onChange={e => setDay(d.key, {working: e.target.checked})}/>
                                    {d.label}
                                </label>
                                {days[d.key].working ? (
                                    <>
                                        <Input type="time" aria-label={`${d.label} start`} className="w-32" value={days[d.key].start}
                                               onChange={e => setDay(d.key, {start: e.target.value})} required/>
                                        <span>–</span>
                                        <Input type="time" aria-label={`${d.label} end`} className="w-32" value={days[d.key].end}
                                               onChange={e => setDay(d.key, {end: e.target.value})} required/>
                                        <span className="ml-auto text-muted-foreground">{formatHours(dayHours(days[d.key]))}</span>
                                    </>
                                ) : (
                                    <span className="text-muted-foreground">Day off</span>
                                )}
                            </div>
                        ))}
                        <div className="flex justify-end text-sm font-semibold text-[#023459]">
                            {formatHours(weeklyHours)} per week
                        </div>
                    </div>

                    <div className="grid gap-2">
                        <Label htmlFor="scheduleFrom">Applies from</Label>
                        <Input id="scheduleFrom" type="date" value={effectiveFrom}
                               min={hasSchedule ? today() : startDate}
                               onChange={e => setEffectiveFrom(e.target.value)} required/>
                    </div>

                    {errors.length > 0 && (
                        <ul role="alert" className="text-sm text-destructive list-disc pl-5">
                            {errors.map(e => <li key={e}>{e}</li>)}
                        </ul>
                    )}

                    <DialogFooter>
                        <DialogClose render={<Button variant="outline" type="button">Close</Button>}/>
                        <Button type="submit" disabled={submitting}>Save schedule</Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}