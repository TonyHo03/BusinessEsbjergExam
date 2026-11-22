"use client"

import * as React from "react"
import { cn } from "cn"
import {
  ChevronDownIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
} from "lucide-react"
import {
  DayPicker,
  getDefaultClassNames,
  type DayButton,
  type Locale,
} from "react-day-picker"

import { Button, buttonVariants } from "@/components/ui/button"
import type {DayOffDto, WorkSchedulesResponse} from "@/api/Api.ts";
import {key, workdayFor} from "@/pages/requestHoliday.tsx";

function Calendar({
                    className,
                    classNames,
                    showOutsideDays = true,
                    captionLayout = "label",
                    buttonVariant = "ghost",
                    locale,
                    formatters,
                    components,
                    ...props
                  }: React.ComponentProps<typeof DayPicker> & {
  buttonVariant?: React.ComponentProps<typeof Button>["variant"]
}) {
  const defaultClassNames = getDefaultClassNames()

  return (
      <DayPicker
          showOutsideDays={showOutsideDays}
          className={cn(
              "group/calendar bg-background p-2 [--cell-radius:var(--radius-md)] [--cell-size:--spacing(7)] in-data-[slot=card-content]:bg-transparent in-data-[slot=popover-content]:bg-transparent",
              String.raw`rtl:**:[.rdp-button\_next>svg]:rotate-180`,
              String.raw`rtl:**:[.rdp-button\_previous>svg]:rotate-180`,
              className
          )}
          captionLayout={captionLayout}
          locale={locale}
          formatters={{
            formatMonthDropdown: (date) =>
                date.toLocaleString(locale?.code, { month: "short" }),
            ...formatters,
          }}
          classNames={{
            root: cn("w-fit", defaultClassNames.root),
            months: cn(
                "relative flex flex-col gap-4 md:flex-row",
                defaultClassNames.months
            ),
            month: cn("flex w-full flex-col gap-4", defaultClassNames.month),
            nav: cn(
                "absolute inset-x-0 top-0 flex w-full items-center justify-between gap-1",
                defaultClassNames.nav
            ),
            button_previous: cn(
                buttonVariants({ variant: buttonVariant }),
                "size-(--cell-size) p-0 select-none aria-disabled:opacity-50",
                defaultClassNames.button_previous
            ),
            button_next: cn(
                buttonVariants({ variant: buttonVariant }),
                "size-(--cell-size) p-0 select-none aria-disabled:opacity-50",
                defaultClassNames.button_next
            ),
            month_caption: cn(
                "flex h-(--cell-size) w-full items-center justify-center px-(--cell-size)",
                defaultClassNames.month_caption
            ),
            dropdowns: cn(
                "flex h-(--cell-size) w-full items-center justify-center gap-1.5 text-sm font-medium",
                defaultClassNames.dropdowns
            ),
            dropdown_root: cn(
                "cn-calendar-dropdown-root relative rounded-(--cell-radius)",
                defaultClassNames.dropdown_root
            ),
            dropdown: cn(
                "absolute inset-0 bg-popover opacity-0",
                defaultClassNames.dropdown
            ),
            caption_label: cn(
                "font-medium select-none",
                captionLayout === "label"
                    ? "cn-calendar-caption text-sm"
                    : "cn-calendar-caption-label flex items-center gap-1 rounded-(--cell-radius) text-sm [&>svg]:size-3.5 [&>svg]:text-muted-foreground",
                defaultClassNames.caption_label
            ),
            month_grid: cn("w-full border-collapse", defaultClassNames.month_grid),
            weekdays: cn("flex", defaultClassNames.weekdays),
            weekday: cn(
                "flex-1 rounded-(--cell-radius) text-[0.8rem] font-normal text-muted-foreground select-none",
                defaultClassNames.weekday
            ),
            week: cn("mt-2 flex w-full", defaultClassNames.week),
            week_number_header: cn(
                "w-(--cell-size) select-none",
                defaultClassNames.week_number_header
            ),
            week_number: cn(
                "text-[0.8rem] text-muted-foreground select-none",
                defaultClassNames.week_number
            ),
            day: cn(
                "group/day relative aspect-square h-full w-full rounded-(--cell-radius) p-0 text-center select-none [&:last-child[data-selected=true]_button]:rounded-r-(--cell-radius)",
                props.showWeekNumber
                    ? "[&:nth-child(2)[data-selected=true]_button]:rounded-l-(--cell-radius)"
                    : "[&:first-child[data-selected=true]_button]:rounded-l-(--cell-radius)",
                defaultClassNames.day
            ),
            range_start: cn(
                "relative isolate z-0 rounded-l-(--cell-radius) bg-muted after:absolute after:inset-y-0 after:right-0 after:w-4 after:bg-muted",
                defaultClassNames.range_start
            ),
            range_middle: cn("rounded-none", defaultClassNames.range_middle),
            range_end: cn(
                "relative isolate z-0 rounded-r-(--cell-radius) bg-muted after:absolute after:inset-y-0 after:left-0 after:w-4 after:bg-muted",
                defaultClassNames.range_end
            ),
            today: cn(
                "rounded-(--cell-radius) bg-muted text-foreground data-[selected=true]:rounded-none",
                defaultClassNames.today
            ),
            outside: cn(
                "text-muted-foreground aria-selected:text-muted-foreground",
                defaultClassNames.outside
            ),
            disabled: cn(
                "text-muted-foreground opacity-50",
                defaultClassNames.disabled
            ),
            hidden: cn("invisible", defaultClassNames.hidden),
            ...classNames,
          }}
          components={{
            Root: ({ className, rootRef, ...props }) => {
              return (
                  <div
                      data-slot="calendar"
                      ref={rootRef}
                      className={cn(className)}
                      {...props}
                  />
              )
            },
            Chevron: ({ className, orientation, ...props }) => {
              if (orientation === "left") {
                return (
                    <ChevronLeftIcon
                        className={cn("cn-rtl-flip size-4", className)}
                        {...props}
                    />
                )
              }

              if (orientation === "right") {
                return (
                    <ChevronRightIcon
                        className={cn("cn-rtl-flip size-4", className)}
                        {...props}
                    />
                )
              }

              return (
                  <ChevronDownIcon className={cn("size-4", className)} {...props} />
              )
            },
            DayButton: ({ ...props }) => (
                <CalendarDayButton locale={locale} {...props} />
            ),
            WeekNumber: ({ children, ...props }) => {
              return (
                  <td {...props}>
                    <div className="flex size-(--cell-size) items-center justify-center text-center">
                      {children}
                    </div>
                  </td>
              )
            },
            ...components,
          }}
          {...props}
      />
  )
}



function CalendarDayButton({
                               className,
                               day,
                               modifiers,
                               locale,
                               ...props
                           }: React.ComponentProps<typeof DayButton> & { locale?: Partial<Locale> }) {
    const defaultClassNames = getDefaultClassNames()

    const ref = React.useRef<HTMLButtonElement>(null)
    React.useEffect(() => {
        if (modifiers.focused) ref.current?.focus()
    }, [modifiers.focused])

    // Check if Saturday (6) or Sunday (0) — or use your existing weekend variable
    const isWeekend = day.date.getDay() === 0 || day.date.getDay() === 6

    return (
        <Button
            variant="ghost"
            size="icon"
            data-day={day.date.toLocaleDateString(locale?.code)}
            data-selected-single={
                !isWeekend &&
                modifiers.selected &&
                !modifiers.range_start &&
                !modifiers.range_end &&
                !modifiers.range_middle
            }
            data-range-start={!isWeekend && modifiers.range_start}
            data-range-end={!isWeekend && modifiers.range_end}
            data-range-middle={!isWeekend && modifiers.range_middle}
            className={cn(
                "relative isolate z-10 flex aspect-square size-auto w-full min-w-(--cell-size) flex-col gap-1 border-0 leading-none font-normal group-data-[focused=true]/day:relative group-data-[focused=true]/day:z-10 group-data-[focused=true]/day:border-ring group-data-[focused=true]/day:ring-[3px] group-data-[focused=true]/day:ring-ring/50 data-[range-end=true]:rounded-(--cell-radius) data-[range-end=true]:rounded-r-(--cell-radius) data-[range-end=true]:bg-primary data-[range-end=true]:text-primary-foreground data-[range-middle=true]:rounded-none data-[range-middle=true]:bg-muted data-[range-middle=true]:text-foreground data-[range-start=true]:rounded-(--cell-radius) data-[range-start=true]:rounded-l-(--cell-radius) data-[range-start=true]:bg-primary data-[range-start=true]:text-primary-foreground data-[selected-single=true]:bg-primary data-[selected-single=true]:text-primary-foreground dark:hover:text-foreground [&>span]:text-xs [&>span]:opacity-70 overflow",
                isWeekend && "bg-transparent text-muted-foreground opacity-50",
                defaultClassNames.day,
                className
            )}
            {...props}
        />
    )
}
type Props = React.ComponentProps<typeof DayButton> & {
    schedule: WorkSchedulesResponse | null;
    holidays: Map<string, DayOffDto>;
};

function HolidayDayButton({ day, modifiers, className, schedule, holidays, ...props }: Props) {

    const ref = React.useRef<HTMLButtonElement>(null);
    React.useEffect(() => {
        if (modifiers.focused) ref.current?.focus();
    }, [modifiers.focused]);

    const date = day.date;
    const isHoliday = modifiers.holiday;
    const hours = date.getDay() === 5 ? "7 h" : "7.5 h";

    const off = holidays.get(key(day.date));        // the DTO, or undefined
    const wd = workdayFor(day.date, schedule);
    const isWeekend = day.date.getDay() === 0 || day.date.getDay() === 6;
    const picked = !!wd && !off && modifiers.selected;
    const working = !isHoliday && !isWeekend;

    const sub = isWeekend ? "Weekend"
        : off ? (off.isCompany ? "Company day off" : off.name)
            : !wd ? "Day off"
                : picked ? `${wd.hours} h`
                    : "";

    return (
        <button
            ref={ref}
            {...props}
            title={off ? off.name : undefined}
            className={cn(
                "flex h-full w-full flex-col items-start justify-between rounded-[10px] border p-3 text-left text-base font-medium",
                "focus-visible:ring-2 focus-visible:ring-ring",
                working && !picked && "border-border bg-white hover:bg-muted",
                picked && "border-primary bg-primary text-primary-foreground",
                isWeekend && "border-border bg-[#F4EDE0] text-muted-foreground",
                isHoliday && "border-[#D5CABB] bg-[#E9E1D6]",
                modifiers.outside && "opacity-50",
                className,
            )}
        >
            <span>{date.getDate()}</span>
            <span className="text-xs font-normal">{sub}</span>
        </button>
    );
}

export { Calendar, CalendarDayButton, HolidayDayButton }
