import type { ButtonHTMLAttributes } from "react";

/** Primær knap: terracotta-orange, 42 px høj, afrundede hjørner. */
export function Button({ className = "", ...props }: ButtonHTMLAttributes<HTMLButtonElement>) {
    return (
        <button
            type="button"
            className={`inline-flex h-[42px] items-center justify-center gap-2 rounded-lg bg-accent-strong px-4 text-[13px] font-semibold text-white hover:bg-accent-strong/90 disabled:opacity-50 ${className}`}
            {...props}
        />
    );
}