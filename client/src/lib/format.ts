import type {HolidayAccount} from "@/types/approvals.ts";

const MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
const ACCOUNT_LABELS: Record<HolidayAccount, string> = {Weeks1To5: 'Weeks 1-5', Week6: '6th week'};

export const accountLabel = (account: HolidayAccount) => ACCOUNT_LABELS[account];
export const formatHours = (hours: number) => `${Number.isInteger(hours) ? hours : hours.toFixed(1)} h`;

export const formatDays = (days: number) => `${days} ${days === 1 ? 'day' : 'days'}`;

const pad = (n: number) => String(n).padStart(2, '0');

function parts(iso: string) {
    const [y = 0, m = 1, d = 1] =iso.slice(0, 10).split('-').map(Number);
    return {y, month: MONTHS[m - 1] ?? '', d: pad(d)};
}

export function formatDate(iso: string) {
    const {y, month, d} = parts(iso);
    return `${d} ${month} ${y}`;
}

export function formatDateRange(startIso: string, endIso: string){
    const s = parts(startIso)
    const e = parts(endIso)
    if (s.y === e.y && s.month === e.month) return `${s.d}-${e.d} ${s.month} ${s.y}`;
    if (s.y === e.y) return `${s.d} ${s.month} ${e.d} ${e.month} ${s.y}`;
    return `${s.d} ${s.month} ${s.y} ${e.d} ${e.month} ${e.y}`
}
export function initials(name: string): string {
    const words = name.trim().split(/\s+/).filter(Boolean);
    return ((words[0]?.[0] ?? '') + (words.length > 1 ? (words.at(-1)?.[0] ?? '') : '')).toUpperCase();

}
