export const REASON_MAX_LENGTH = 300;

export type HolidayAccount = 'Weeks1To5' | 'Week6'

export interface PendingRequestDto {
    requestId: string;
    employeeId: string;
    employeeName: string;
    startDate: string;
    endDate: string;
    workingDays: number;
    hours: number;
    account: HolidayAccount;
    submittedAt: string;
    note?: string | null;
    isOwnRequest: boolean;
}