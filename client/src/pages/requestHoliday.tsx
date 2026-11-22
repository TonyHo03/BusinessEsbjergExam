import {Label} from "@/components/ui/label.tsx";
import {Separator} from "@/components/ui/separator.tsx";
import * as React from "react";
import {useEffect, useMemo, useState} from "react";
import {Card, CardContent} from "@/components/ui/card.tsx";
import {Calendar, HolidayDayButton} from "../components/ui/calendar"
import type {DateRange, DayButton} from "react-day-picker";
import {Badge} from "@/components/ui/badge.tsx";
import {Select, SelectContent, SelectItem, SelectTrigger, SelectValue} from "@/components/ui/select.tsx";
import {
    type BalancesResponse,
    type CurrentLoggedUser, type DayOffDto,
    type EmployeeDto,
    HolidayAccount,
    type WorkSchedulesResponse
} from "@/api/Api.ts";
import {api} from "@/api/client.ts";
import {Input} from "@/components/ui/input";
import {ArrowLeft} from "lucide-react";
import {useAuth} from "@/components/authProvider.tsx";
import {formatHours} from "@/lib/format.ts";
import {addMonths, format, isBefore, startOfDay} from "date-fns";
import {ErrorPanel} from "@/components/StatePanels.tsx";
import {useNavigate} from "react-router";

const locale = "en-US";

export const key = (d: Date) =>
    `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;

export function businessDays(
    range: DateRange | undefined,
    schedule: WorkSchedulesResponse | null,
    daysOff: Map<string, DayOffDto>,
): Date[] {
    if (!range?.from || !range.to) return [];
    const days: Date[] = [];
    const d = new Date(range.from.getFullYear(), range.from.getMonth(), range.from.getDate());
    const end = new Date(range.to.getFullYear(), range.to.getMonth(), range.to.getDate());

    while (d <= end) {
        if (hoursFor(d, schedule, daysOff) > 0) days.push(new Date(d));   // <- here
        d.setDate(d.getDate() + 1);
    }
    return days;
}

const WEEKDAYS = [undefined, "monday", "tuesday", "wednesday", "thursday", "friday"] as const;

const toMinutes = (t: string) => {
    const [h, m] = t.split(":").map(Number);
    return h! * 60 + m!;
};

const day10 = (s?: string | null) => (s ?? "").slice(0, 10); // tolerate ISO datetimes

export type Workday = { hours: number; start: string; end: string };

export function workdayFor(date: Date, res: WorkSchedulesResponse | null): Workday | null {
    if (!res) return null;
    const k = key(date);

    if (res.employeeStartDate && k < day10(res.employeeStartDate)) return null;

    const schedule = res.schedules?.find(
        (s) => day10(s.effectiveFrom) <= k && (!s.effectiveTo || k <= day10(s.effectiveTo)),
    );

    const name = WEEKDAYS[date.getDay()];
    const times = name ? schedule?.[name] : undefined;
    if (!times) return null;

    const hours = (toMinutes(times.end!) - toMinutes(times.start!)) / 60;
    return hours > 0 ? { hours, start: times.start!, end: times.end! } : null;
}

export const hoursFor = (
    d: Date,
    schedule: WorkSchedulesResponse | null,
    daysOff: Map<string, DayOffDto>,
) => (daysOff.has(key(d)) ? 0 : (workdayFor(d, schedule)?.hours ?? 0));


export function RequestHolidayView() {

    const date = new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate());
    const [workSchedule, setWorkSchedule] = useState<WorkSchedulesResponse | null>(null);
    const [holidays, setHolidays] = useState<DayOffDto[]>([])
    const [comment, setComment] = useState<string>("");


    const daysOffByDate = useMemo(
        () => new Map(holidays.map((d) => [d.date!.slice(0, 10), d])),
        [holidays],
    );

    const DayButtonWithSchedule = useMemo(
        () => (props: React.ComponentProps<typeof DayButton>) =>
            <HolidayDayButton {...props} schedule={workSchedule} holidays={daysOffByDate} />,
        [workSchedule, daysOffByDate],
    );

    const {user} = useAuth();

    const [range, setRange] = React.useState<DateRange | undefined>(undefined)

    useEffect(() => {

        api.api.workSchedulesGet({employeeId: user?.id!}).then(setWorkSchedule).catch(err => console.log(err));
        api.api.daysOffGet({year: date.getFullYear()}).then(setHolidays).catch(err => console.log(err));

    }, []);


    return (
        <div className="flex flex-col w-full h-full p-12 gap-4">
            <div className="flex w-full">
                <Label className="text-[#0F2338] text-3xl font-normal">Request Holidays</Label>
            </div>
            <Label className="text-[#516080]">Make space for your next break. Select your dates and choose a holiday account.</Label>
            <Separator className="h-px"/>
            <div className="flex w-full gap-8">
                <div className="flex flex-col w-full gap-3">
                    <HolidayCalendar range={range} setRange={setRange} workSchedule={workSchedule} daysOffByDate={daysOffByDate} DayButtonWithSchedule={DayButtonWithSchedule}/>
                    <div className="flex flex-col w-full gap-1.5">
                        <p className="text-sm font-semibold text-foreground">Note to accounting (optional)</p>
                        <Input value={comment} onChange={e => setComment(e.target.value)} placeholder="Add a short note..." className="bg-card! h-10"/>
                    </div>
                </div>
                <SubmitCard range={range} user={user} workSchedule={workSchedule} daysOffByDate={daysOffByDate} comment={comment}/>
            </div>
        </div>
    );
}

function HolidayCalendar({range, setRange, workSchedule, daysOffByDate, DayButtonWithSchedule}: {range: DateRange | undefined, setRange: (range: DateRange | undefined) => void, workSchedule: WorkSchedulesResponse | null, daysOffByDate: Map<string, DayOffDto>, DayButtonWithSchedule: (props: React.ComponentProps<typeof DayButton>) => React.JSX.Element}) {

    return (
        <Card className="w-full p-0 px-8 pb-8">
            <CardContent className="flex flex-col p-0">
                <Calendar
                    mode="range"
                    defaultMonth={range?.from}
                    startMonth={new Date()}
                    endMonth={new Date(new Date().getFullYear() + 5, 11, 31)}
                    selected={range}
                    onSelect={setRange}
                    numberOfMonths={1}
                    captionLayout="dropdown"
                    weekStartsOn={1}
                    modifiers={{weekend: {dayOfWeek: [0,6]}}}
                    modifiersClassNames={{weekend: "bg-muted border-1 border-primary text-muted-foreground rounded-lg!"}}
                    disabled={(d) => hoursFor(d, workSchedule, daysOffByDate) === 0}
                    className="w-full p-0 [--cell-size:--spacing(22)]"
                    classNames={{
                        root: "relative w-full",
                        months: "w-full",
                        month: "w-full gap-4",
                        month_grid: "w-full [&_thead]:block [&_tbody]:block",
                        weekdays: "grid grid-cols-7 gap-2",
                        weekday: "text-center",
                        week: "mt-2 grid grid-cols-7 gap-2",
                        day: "aspect-auto h-(--cell-size) w-full",
                        range_start: "", range_middle: "", range_end: "",
                    }}
                    formatters={{
                        formatMonthDropdown: (date) => {
                            return date.toLocaleString("default", { month: "long" })
                        },
                    }}
                    components={{ DayButton: DayButtonWithSchedule }}
                />
            </CardContent>
        </Card>
    );
}

function SubmitCard({range, user, workSchedule, daysOffByDate, comment}: {range: DateRange | undefined, user: CurrentLoggedUser | null, workSchedule: WorkSchedulesResponse | null, daysOffByDate: Map<string, DayOffDto>, comment: string}) {

    const [account, setAccount] = useState<HolidayAccount | null>(null);
    const [balance, setBalance] = useState<BalancesResponse | null>(null);
    const [error, setError] = useState<boolean>(false);
    const [submitting, setSubmitting] = useState<boolean>(false);
    const navigate = useNavigate();

    const summary = useMemo(() => {
        const days = businessDays(range, workSchedule, daysOffByDate);

        const perDay = days.map((d) => ({ date: d, hours: hoursFor(d, workSchedule, daysOffByDate) }));
        const totalHours = perDay.reduce((sum, d) => sum + d.hours, 0);

        const groups = new Map<number, number>();
        for (const { hours } of perDay) groups.set(hours, (groups.get(hours) ?? 0) + 1);
        const breakdown = [...groups]
            .map(([h, n]) => (n > 1 ? `${n} × ${h} h` : `${h} h`))
            .join(" + ");

        return { workingDays: days.length, totalHours, breakdown };
    }, [range, workSchedule, daysOffByDate]);



    useEffect(() => {

        api.api.balancesGet({employeeId: user?.id!, asOf: new Date().toLocaleDateString(locale)}).then(setBalance).catch(err => console.log(err))

    }, []);

    const available = account == HolidayAccount.Weeks1To5 ? balance?.availableWeeks1To5! : account == HolidayAccount.Week6 ? balance?.availableWeek6! : undefined;
    const isEligible = (available! - summary.totalHours) >= 0;

    const handeSubmit = async () => {
        setSubmitting(true);
        if (!isEligible || submitting) {
            return;
        }

        setError(false);

        try {
            await api.api.requestsSubmit({employeeId: user?.id!, account: account ?? undefined, startDate: format(range?.from!, "yyyy-MM-dd"), endDate: format(range?.to!, "yyyy-MM-dd"), comment: comment});
            navigate("/my-requests");
        }
        catch (err) {
            setError(true);
            setSubmitting(false);
            console.log(err);
        }
    }

    return (
        <Card className="min-h-0 self-start">
            <CardContent className="flex flex-col max-w-full w-125 gap-6 shrink-0">
                <div className="flex w-full items-center">
                    <p className="text-lg text-foreground font-medium">Your request</p>
                    <Badge variant={"secondary"} className="ml-auto">Draft</Badge>
                </div>
                <div className="flex flex-col gap-2">
                    <p className="text-muted-foreground uppercase">Selected Dates</p>
                    <p className="text-2xl text-foreground font-semibold">{range && range.to && range.from ? `${range.from.getDate()} ${range.from.toLocaleString(locale, {month: "short"})} ${range.from.getFullYear()} - ${range.to.getDate()} ${range.to.toLocaleString(locale, {month: "short"})} ${range.to.getFullYear()}` : "Unknown"}</p>
                    <p className="text-muted-foreground">{`${ range?.from && range.to ? range?.from?.toLocaleString(locale, {weekday: "long"}) + " to " + range?.to?.toLocaleString(locale, {weekday: "long"}) : "Select your dates"}`}</p>
                </div>
                <div className="flex gap-12">
                    <div className="flex flex-col gap-2">
                        <p className="text-2xl text-foreground font-semibold">{summary.workingDays}</p>
                        <p className="text-muted-foreground">Working days</p>
                    </div>
                    <div className="flex flex-col gap-2">
                        <p className="text-2xl text-foreground font-semibold">{summary.totalHours}</p>
                        <p className="text-muted-foreground">{summary.breakdown}</p>
                    </div>
                </div>
                <Separator className="h-px"/>
                <div className="flex flex-col gap-2">
                    <p className="font-semibold text-foreground">Holiday account</p>
                    <Select value={account} onValueChange={setAccount}>
                        <SelectTrigger className="w-full min-w-0 bg-card! h-10!">
                            <SelectValue placeholder={"Choose an account"}/>
                        </SelectTrigger>
                        <SelectContent>
                            {Object.values(HolidayAccount).map(h => <SelectItem key={h} value={h}>{h}</SelectItem>)}
                        </SelectContent>
                    </Select>
                </div>
                <div className="flex flex-col gap-4">
                    <div className="flex">
                        <p className="text-muted-foreground">Available now: </p>
                        <p className="text-foreground font-semibold ml-auto">{account == HolidayAccount.Weeks1To5 ? balance?.availableWeeks1To5 : account == HolidayAccount.Week6 ? balance?.availableWeek6 : "Unknown"} h</p>
                    </div>
                    <div className="flex">
                        <p className="text-muted-foreground">This request: </p>
                        <p className="text-foreground font-semibold ml-auto">{summary.totalHours}</p>
                    </div>
                    <div className="flex">
                        <p className="text-foreground font-semibold">Remaining after request:</p>
                        <p className={`${(available! - summary.totalHours) ? "text-success-fg" : "text-destructive-fg"} font-semibold ml-auto`}>{formatHours(available! - summary.totalHours)}</p>
                    </div>
                </div>
                {error && <ErrorPanel title={"Unexpected error"} message={"Please make sure you have selected your holiday dates, selected holiday account, and made sure your holiday is set one month in advance."} onRetry={handeSubmit}/>}
                {!error && <button onClick={handeSubmit} className="flex justify-center items-center rounded-md py-3 gap-2 bg-primary font-semibold text-primary-foreground"><ArrowLeft/> Submit request</button>}
            </CardContent>
        </Card>
    );
}