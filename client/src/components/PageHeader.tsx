import type { ReactNode } from "react";

type Props = {
    title: string;
    description?: string;
    /** Knapper til højre, fx "Request holiday" */
    actions?: ReactNode;
};

export function PageHeader({ title, description, actions }: Props) {
    return (
        <header className="mb-7 flex flex-wrap items-center justify-between gap-4">
            <div className="flex min-w-0 flex-1 flex-col gap-1.5 leading-normal">
                <h1 className="text-[30px] text-ink">{title}</h1>
                {description && <p className="text-sm text-muted">{description}</p>}
            </div>
            {actions && <div className="flex gap-2">{actions}</div>}
        </header>
    );
}