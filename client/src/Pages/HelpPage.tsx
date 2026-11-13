import type { LucideIcon } from "lucide-react";
import { CalendarDays, CalendarPlus, HeartPulse, Info, Sun, Umbrella, Wallet } from "lucide-react";
import { PageHeader } from "../components/PageHeader";

type PolicySection = { title: string; icon: LucideIcon; points: string[] };

// Business Esbjergs regler: Ferieloven + 6. ferieuge (KL 05.11).
// Tallene står her som tekst. Når admin-siden for parametre (#17) er lavet, kan de hentes fra API'et i stedet.
const policy: PolicySection[] = [
    {
        title: "Weeks 1–5 (Holiday Act)",
        icon: CalendarDays,
        points: [
            "You earn 2.08 days per month of employment – 25 days (185 h) a year when full time.",
            "Earned from 1 September to 31 August.",
            "Can be used as soon as it is earned, until 31 December the following year.",
        ],
    },
    {
        title: "6th holiday week",
        icon: Sun,
        points: [
            "You earn 3.083 h per month – 37 h (one full week) a year when full time.",
            "Earned in the calendar year and used from 1 May to 30 April the following year.",
            "Placed at your request, unless work makes it impossible. Kept separate from weeks 1–5.",
        ],
    },
    {
        title: "Requesting holiday",
        icon: CalendarPlus,
        points: [
            "Request whole days. Each day costs the hours of that weekday: Mon–Thu 7.5 h, Fri 7 h.",
            "Weekends, public holidays and company days off (24 and 31 December) cost nothing.",
            "Give at least 1 month's notice – 3 months for your main holiday.",
            "Where possible, take holiday in blocks of at least 5 days.",
        ],
    },
    {
        title: "Main holiday",
        icon: Umbrella,
        points: [
            "You have the right to 15 consecutive working days off between 1 May and 30 September.",
            "Request it at least 3 months in advance.",
        ],
    },
    {
        title: "Holiday you don't use",
        icon: Wallet,
        points: [
            "The first 4 weeks must be taken as time off and cannot be paid out while you are employed.",
            "The 5th week can be transferred by written agreement by 31 December. Otherwise it is paid out.",
            "6th week hours not used by 30 April are paid out, unless transferred by agreement before 30 September.",
            "When you leave, all remaining holiday is settled with your final salary.",
        ],
    },
    {
        title: "Illness and other obstacles",
        icon: HeartPulse,
        points: [
            "If you are ill before your holiday starts, you don't have to start it and can take the days later.",
            "If you fall ill during holiday (weeks 1–5), you can get replacement days after the first 5 sick days, with documentation.",
            "Parental leave and similar events also protect your holiday. Contact accounting.",
        ],
    },
];

export function HelpPage() {
    return (
        <>
            <PageHeader
                title="Help & company policy"
                description="How holiday works at Business Esbjerg – the Danish Holiday Act plus the 6th holiday week."
            />

            <div className="grid gap-5 lg:grid-cols-2">
                {policy.map(({ title, icon: Icon, points }) => (
                    <section key={title} className="flex flex-col gap-4 rounded-xl border border-line bg-white p-[26px]">
                        <h2 className="flex items-center gap-2.5 text-[17px] leading-normal font-semibold text-ink">
                            <Icon className="size-[18px] shrink-0 text-brand" aria-hidden />
                            {title}
                        </h2>
                        <ul className="flex list-disc flex-col gap-2 pl-5 text-[13px] leading-normal text-muted marker:text-subtle">
                            {points.map((point) => (
                                <li key={point}>{point}</li>
                            ))}
                        </ul>
                    </section>
                ))}
            </div>

            <div className="mt-7 flex items-start gap-2.5 rounded-lg bg-brand-soft p-[14px] text-xs leading-normal text-brand">
                <Info className="size-[18px] shrink-0" aria-hidden />
                <p>
                    This is a summary. The Danish Holiday Act (Ferieloven) and the 6th week agreement (KL 05.11) always apply.
                    Questions about your own holiday? Contact accounting.
                </p>
            </div>
        </>
    );
}