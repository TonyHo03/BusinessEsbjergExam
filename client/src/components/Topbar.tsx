import { useLocation } from "react-router";
import { Bell, ChevronRight } from "lucide-react";
import { findCurrent } from "./navigation";

const today = new Intl.DateTimeFormat("en-GB", { day: "2-digit", month: "short", year: "numeric" }).format(new Date());

export function Topbar() {
    const { pathname } = useLocation();
    const { item, workspace } = findCurrent(pathname);

    return (
        <header className="flex h-[72px] shrink-0 items-center justify-between gap-4 border-b border-line bg-white px-6 md:px-10">
            <nav aria-label="Breadcrumb" className="flex items-center gap-2.5 text-[13px]">
                <span className="text-muted">Holiday Planner</span>
                <ChevronRight className="size-[13px] text-muted" aria-hidden />
                <span className="font-medium text-ink">{item?.label ?? "Not found"}</span>
            </nav>

            <div className="flex items-center gap-5">
                <span className="hidden text-xs text-muted sm:inline">{today}</span>
                <span className="rounded-full bg-brand-soft px-[9px] py-1 text-xs font-semibold text-brand">
          {workspace} workspace
        </span>
                <button type="button" aria-label="Notifications" className="text-muted hover:text-ink">
                    <Bell className="size-[18px]" />
                </button>
            </div>
        </header>
    );
}