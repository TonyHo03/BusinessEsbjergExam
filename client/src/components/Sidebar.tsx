import { NavLink } from "react-router";
import { CircleHelp, LogOut } from "lucide-react";
import { sections } from "./navigation";
import logo from "../assets/logo.svg";

export function Sidebar() {
    return (
        <aside className="flex flex-col justify-between gap-8 border-b border-line bg-white px-5 pt-[30px] pb-[22px] md:sticky md:top-0 md:h-screen md:w-60 md:shrink-0 md:border-r md:border-b-0">
            <div className="flex flex-col gap-[30px]">
                {/* Logo og app-navn */}
                <div className="flex flex-col gap-[23px] px-2.5">
                    <img src={logo} alt="Business Esbjerg" className="h-9 w-auto self-start" />
                    <p className="text-base text-ink">Holiday Planner</p>
                </div>

                {/* Menu: 30 px mellem grupperne, 8 px mellem punkterne */}
                <nav aria-label="Main navigation" className="flex flex-col gap-[30px]">
                    {sections.map((section) => (
                        <div key={section.workspace} className="flex flex-col gap-2">
                            <p className="px-3 text-[10px] leading-normal font-bold text-subtle uppercase">{section.workspace}</p>
                            {section.items.map(({ to, label, icon: Icon, end, badge }) => (
                                <NavLink
                                    key={to}
                                    to={to}
                                    end={end}
                                    className={({ isActive }) =>
                                        [
                                            "flex h-[42px] items-center gap-[11px] rounded-lg px-3 text-[13px] leading-normal transition-colors",
                                            isActive ? "bg-brand-soft font-semibold text-brand" : "text-muted hover:bg-canvas",
                                        ].join(" ")
                                    }
                                >
                                    <Icon className="size-[18px] shrink-0" aria-hidden />
                                    {label}
                                    {badge !== undefined && (
                                        <span className="rounded-full bg-brand-soft px-[9px] py-1 text-xs font-semibold text-brand">
                      {badge}
                    </span>
                                    )}
                                </NavLink>
                            ))}
                        </div>
                    ))}
                </nav>
            </div>

            {/* Hjælp og bruger */}
            <div className="hidden flex-col gap-[18px] md:flex">
                <NavLink to="/help" className="flex items-start gap-2.5 px-3 text-[13px] leading-normal text-muted hover:text-ink">
                    <CircleHelp className="size-[18px] shrink-0" aria-hidden />
                    Help &amp; company policy
                </NavLink>

                <div className="h-px bg-line" />

                <div className="flex items-start gap-2.5">
          <span className="grid size-[34px] shrink-0 place-items-center rounded-full bg-brand-soft text-xs font-semibold text-brand">
            MJ
          </span>
                    <span className="flex min-w-0 flex-1 flex-col gap-px leading-normal">
            <span className="text-xs font-semibold text-ink">Mette Jensen</span>
            <span className="text-[11px] text-muted">Employee</span>
          </span>
                    <button type="button" aria-label="Sign out" className="text-muted hover:text-ink">
                        <LogOut className="size-4" />
                    </button>
                </div>
            </div>
        </aside>
    );
}