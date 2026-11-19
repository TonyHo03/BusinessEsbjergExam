import type { ReactNode } from 'react';
import { Skeleton } from '@/components/ui/skeleton.tsx';

const buttonClass =
    'inline-flex h-9 w-fit items-center justify-center rounded-lg bg-primary px-4 text-sm font-medium text-primary-foreground';

export function LoadingRows({ label }: { label: string }) {
    return (
        <div role="status" aria-live="polite" aria-label={label} className="flex flex-col gap-3">
            <Skeleton className="h-16 w-full rounded-xl" />
            <Skeleton className="h-16 w-full rounded-xl" />
            <Skeleton className="h-16 w-full rounded-xl" />
        </div>
    );
}

export function ErrorPanel({ title, message, onRetry }: { title: string; message: string; onRetry: () => void }) {
    return (
        <div role="alert" className="flex flex-col gap-3 rounded-xl border border-[#CE6B4C] bg-card p-6">
            <h2 className="text-lg font-medium text-[#0F2338]">{title}</h2>
            <p className="text-sm text-[#516080]">{message}</p>
            <button type="button" className={buttonClass} onClick={onRetry}>Try again</button>
        </div>
    );
}

export function EmptyPanel({ title, children }: { title: string; children?: ReactNode }) {
    return (
        <div className="flex flex-col items-center gap-2 rounded-xl border bg-card p-12 text-center">
            <h2 className="text-lg font-medium text-[#0F2338]">{title}</h2>
            <div className="text-sm text-[#516080]">{children}</div>
        </div>
    );
}