import { api } from './client';
import type { PendingRequestDto } from '../types/approvals';

// Kalder det rigtige API. Den generede klient kaster selve Response-objektet ved fejl,
// så vi læser API'ets fejlbesked (ProblemDetails) og laver den om til en almindelig Error.

export function errorMessage(error: unknown): string {
    return error instanceof Error && error.message ? error.message : 'Something went wrong. Please try again.';
}

async function call<T>(request: () => Promise<T>): Promise<T> {
    try {
        return await request();
    } catch (error: any) {
        if (error instanceof Error) throw error; // fx "Failed to fetch"
        const problem = await error?.json?.().catch(() => null);
        throw new Error(problem?.detail ?? problem?.title ?? statusMessage(error?.status));
    }
}

function statusMessage(status?: number): string {
    switch (status) {
        case 401: return 'Your session has expired. Log in again.';
        case 403: return 'Only accounting can approve or reject requests.';
        case 404: return 'The request no longer exists. Refresh the list.';
        default: return 'Something went wrong. Please try again.';
    }
}

export async function getPendingApprovals(): Promise<PendingRequestDto[]> {
    return call(() => api.api.approvalsGetPending()) as Promise<PendingRequestDto[]>;
}

export async function approveRequest(requestId: string): Promise<void> {
    await call(() => api.api.approvalsApprove({ id: requestId }));
}

export async function rejectRequest(requestId: string, reason: string): Promise<void> {
    await call(() => api.api.approvalsReject({ id: requestId }, { reason }));
}