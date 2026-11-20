import {Link, useLocation} from "react-router";
import {ChevronRight} from "lucide-react";
import {Badge} from "@/components/ui/badge.tsx";
import {useAuth} from "@/components/authProvider.tsx";
import {Roles} from "@/api/Api.ts";
import {
    Breadcrumb,
    BreadcrumbItem, BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbPage,
    BreadcrumbSeparator
} from "@/components/ui/breadcrumb.tsx";
import {Fragment} from "react";

const today = new Intl.DateTimeFormat("en-GB", { day: "2-digit", month: "short", year: "numeric" }).format(new Date());

const labels: Record<string, string> = {
    dashboard: "Dashboard",
    "request-holidays": "Request Holidays",
    "my-requests": "My Requests",
    approvals: "Pending Approvals",
    "employee-overview": "Employee Overview",
    "usermanagement": "User Management",
    parameters: "Parameters",
    help: "Help & company policy",
};

export function Topbar() {
    const { pathname } = useLocation();
    const {user} = useAuth();
    const workspace = user?.role ?? Roles.Employee;

    const segments = pathname.split("/").filter(Boolean);

    return (
        <header className="flex sticky top-0 h-[72px] shrink-0 items-center justify-between gap-4 border-b border-line bg-white px-6 md:px-10">
            <div className="flex items-center">
                <p className="text-sm text-foreground mr-1">Holiday Planner /</p>
                <Breadcrumb>
                    <BreadcrumbList>
                        {segments.map((seg, i) => {
                            const to = "/" + segments.slice(0, i + 1).join("/");
                            const label = labels[seg] ?? seg;
                            const isLast = i === segments.length - 1;

                            return (
                                <Fragment key={to}>
                                    <BreadcrumbItem>
                                        {isLast
                                            ? <BreadcrumbPage>{label}</BreadcrumbPage>
                                            : <BreadcrumbLink render={<Link to={to} />}>{label}</BreadcrumbLink>}
                                    </BreadcrumbItem>
                                    {!isLast && <BreadcrumbSeparator />}
                                </Fragment>
                            );
                        })}
                    </BreadcrumbList>
                </Breadcrumb>
            </div>

            <div className="flex items-center gap-5">
                <span className="hidden text-xs text-foreground sm:inline">{today}</span>
                <Badge variant={"secondary"}>{workspace} workspace</Badge>
            </div>
        </header>
    );
}