import type { ReactNode } from 'react';

const buttonClass = 'inline';

export function LoadingRows({ label }: { label: string }) {
    return (
        <div role="status" aria-live="polite" aria-label={label} className="">
            <div className="" />
            <div className="" />
            <div className="" />
        </div>
    );
}

export function ErrorPanel({ title, message, onRetry }: { title: string; message: string; onRetry: () => void }) {
    return (
        <div role="alert" className="">
            <h2 className="">{title}</h2>
            <p className="">{message}</p>
            <button type="button" className={buttonClass} onClick={onRetry}>Try again</button>
        </div>
    );
}

export function EmptyPanel({ title, children }: { title: string; children?: ReactNode }) {
    return (
        <div className="">
            <h2 className="">{title}</h2>
            <div className="">{children}</div>
        </div>
    );
}