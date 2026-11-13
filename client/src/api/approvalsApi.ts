import type { PendingRequestDto } from '../types/approvals';

// The ONLY file that knows whether data is mocked or real. Bun inlines BUN_PUBLIC_* at build time,
// and the flag is off unless it is set, so a production build never shows fake data.
// Last commit of the branch: replace the three functions with calls to the generated client and
// delete everything below the "mock data" line, plus .env.development.
const USE_MOCK = process.env.BUN_PUBLIC_USE_MOCK === 'true';
const LATENCY_MS = 400;

export function errorMessage(error: unknown): string {
    return error instanceof Error && error.message ? error.message : 'Something went wrong. Please try again.';
}

export async function getPendingApprovals(): Promise<PendingRequestDto[]> {
    if (!USE_MOCK) throw notWired();
    return delay(structuredClone(mockPending));
}

export async function approveRequest(requestId: string): Promise<void> {
    if (!USE_MOCK) throw notWired();
    await delay(null);
    removePending(requestId);
}

export async function rejectRequest(requestId: string, reason: string): Promise<void> {
    if (!USE_MOCK) throw notWired();
    await delay(null);
    if (!reason.trim()) throw new Error('A reason is required when rejecting a request.');
    removePending(requestId);
}

// ---------------------------------------------------------------- mock data

const notWired = () => new Error('API client is not wired up yet. See src/api/approvalsApi.ts.');
const delay = <T>(value: T) => new Promise<T>((resolve) => setTimeout(() => resolve(value), LATENCY_MS));

function daysFromToday(days: number): string {
    const date = new Date();
    date.setDate(date.getDate() + days);
    return date.toISOString().slice(0, 10);
}

const sample = (id: number, name: string, start: number, days: number, hours: number, own = false, note: string | null = null): PendingRequestDto => ({
    requestId: `11111111-0000-0000-0000-00000000000${id}`,
    employeeId: `22222222-0000-0000-0000-00000000000${id}`,
    employeeName: name,
    startDate: daysFromToday(start),
    endDate: daysFromToday(start + days - 1),
    workingDays: days,
    hours,
    account: 'Weeks1To5',
    submittedAt: new Date(Date.now() - id * 3 * 86_400_000).toISOString(),
    note,
    isOwnRequest: own,
});

let mockPending: PendingRequestDto[] = [
    sample(3, 'Jonas Madsen', 60, 4, 30, false, 'A few days away before the main event.'),
    sample(2, 'Freja Nielsen', 120, 5, 37, true),
    sample(1, 'Mikkel Sørensen', 35, 5, 37),
];

function removePending(requestId: string): void {
    if (!mockPending.some((r) => r.requestId === requestId)) {
        throw new Error('This request was changed by someone else. Refresh and try again.');
    }
    mockPending = mockPending.filter((r) => r.requestId !== requestId);
}