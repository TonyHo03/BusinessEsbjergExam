import {Label} from "@/components/ui/label.tsx";
import {Separator} from "@/components/ui/separator.tsx";
import * as React from "react";
import {useState} from "react";
import {Card, CardContent} from "@/components/ui/card.tsx";
import {Calendar, CalendarDayButton, HolidayDayButton} from "../components/ui/calendar"
import type {DateRange} from "react-day-picker";
import {Badge} from "@/components/ui/badge.tsx";
import {format} from "date-fns";
import {Select, SelectContent, SelectItem, SelectTrigger, SelectValue} from "@/components/ui/select.tsx";
import {HolidayAccount} from "@/api/Api.ts";
import {api} from "@/api/client.ts";
import { Input } from "@/components/ui/input";
import {ArrowLeft} from "lucide-react";

const locale = "en-US";

const key = (d: Date) =>
    `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;

function businessDays(range: DateRange | undefined, excluded: Set<string>): Date[] {
    if (!range?.from || !range.to) return [];
    const days: Date[] = [];
    const d = new Date(range.from.getFullYear(), range.from.getMonth(), range.from.getDate());
    const end = new Date(range.to.getFullYear(), range.to.getMonth(), range.to.getDate());
    while (d <= end) {
        const dow = d.getDay();
        if (dow !== 0 && dow !== 6 && !excluded.has(key(d))) days.push(new Date(d));
        d.setDate(d.getDate() + 1);
    }
    return days;
}

export function RequestHolidayView() {

    const date = new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate());

    const [range, setRange] = React.useState<DateRange | undefined>({
        from: new Date(date),
        to: new Date(date),
    })

    return (
        <div className="flex flex-col w-full h-full p-12 gap-4">
            <div className="flex w-full">
                <Label className="text-[#0F2338] text-3xl font-normal">Request Holidays</Label>
            </div>
            <Label className="text-[#516080]">Make space for your next break. Select your dates and choose a holiday account.</Label>
            <Separator className="h-px"/>
            <div className="flex w-full gap-8">
                <div className="flex flex-col w-full gap-3">
                    <HolidayCalendar range={range} setRange={setRange}/>
                    <div className="flex flex-col w-full gap-1.5">
                        <p className="text-sm font-semibold text-foreground">Note to accounting (optional)</p>
                        <Input placeholder="Add a short note..." className="bg-card! h-10"/>
                    </div>
                </div>
                <SubmitCard range={range}/>
            </div>
        </div>
    );
}

function HolidayCalendar({range, setRange}: {range: DateRange | undefined, setRange: (range: DateRange | undefined) => void}) {

    const isWeekend = { dayOfWeek: [0, 6] };

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
                    disabled={isWeekend}
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
                    components={{ DayButton: HolidayDayButton }}
                />
            </CardContent>
        </Card>
    );
}

function SubmitCard({range}: {range: DateRange | undefined}) {

    const [account, setAccount] = useState<HolidayAccount | null>(null);



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
                    <p className="text-muted-foreground">{`${range?.from?.toLocaleString(locale, {weekday: "long"})} to ${range?.to?.toLocaleString(locale, {weekday: "long"})}`}</p>
                </div>
                <div className="flex gap-12">
                    <div className="flex flex-col gap-2">
                        <p className="text-2xl text-foreground font-semibold">{businessDays(range, new Set<string>()).length}</p>
                        <p className="text-muted-foreground">Working days</p>
                    </div>
                    <div className="flex flex-col gap-2">
                        <p className="text-2xl text-foreground font-semibold">37 h</p>
                        <p className="text-muted-foreground">4 × 7.5 h + 7 h</p>
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
                        <p className="text-foreground font-semibold ml-auto">111 h</p>
                    </div>
                    <div className="flex">
                        <p className="text-muted-foreground">This request: </p>
                        <p className="text-foreground font-semibold ml-auto">37 h</p>
                    </div>
                    <div className="flex">
                        <p className="text-foreground font-semibold">Remaining after request: </p>
                        <p className="text-success-fg font-semibold ml-auto">74 h</p>
                    </div>
                </div>
                <button className="flex justify-center items-center rounded-md py-3 gap-2 bg-primary font-semibold text-primary-foreground"><ArrowLeft/> Submit request</button>
            </CardContent>
        </Card>
    );
}