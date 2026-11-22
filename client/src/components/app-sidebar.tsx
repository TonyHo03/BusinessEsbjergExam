import * as React from "react"

import {NavEmployee} from "./nav-employee.tsx"
import {NavAccounting} from "./nav-accounting.tsx"
import {NavAdmin} from "./nav-admin.tsx";
import {NavUser} from "@/components/nav-user"
import {
    Sidebar,
    SidebarContent,
    SidebarFooter,
    SidebarHeader,
    SidebarRail,
    SidebarSeparator,
} from "@/components/ui/sidebar"
import {
    CalendarOff,
    CalendarPlus,
    CircleCheck,
    CircleHelp,
    LayoutDashboard,
    ListChecks,
    SlidersHorizontal,
    User,
    Users
} from "lucide-react"
import {useAuth} from "@/components/authProvider.tsx";
import {NavLink} from "react-router";
import logo from "../assets/logo.svg";
import {Roles} from "@/api/Api.ts";
import {hasRole} from "@/lib/utils.ts";

export const routes = {
    dashboard: "/dashboard",
    requestHolidays: "/request-holidays",
    myRequests: "/my-requests",
    pendingApprovals: "/approvals",
    employeeOverview: "/employee-overview",
    userManagement: "/usermanagement",
    parameters: "/parameters",
    daysOff: "/days-off",
    helpAndPolicies: "/help"
}

export function AppSidebar({ ...props }: React.ComponentProps<typeof Sidebar>) {

    const {user} = useAuth();

    const employeeItems = [
        {title: "Dashboard", url: routes.dashboard, icon: <LayoutDashboard/>},
        {title: "Request Holidays", url: routes.requestHolidays, icon: <CalendarPlus/>},
        {title: "My requests", url: routes.myRequests, icon: <ListChecks/>}
    ]

    const accountingItems = [
        {title: "Pending Approvals", url: routes.pendingApprovals, icon: <CircleCheck/>},
        {title: "Employee overview", url: routes.employeeOverview, icon: <Users/>}
    ]

    const adminItems = [
        {title: "User Management", url: routes.userManagement, icon: <User/>},
        {title: "Parameters", url: routes.parameters, icon: <SlidersHorizontal/>},
        {title: "Days off", url: routes.daysOff, icon: <CalendarOff/>}
    ]

    return (
        <Sidebar collapsible="icon" {...props}>
            <SidebarHeader className="px-6 pt-6 gap-6">
                {/*<TeamSwitcher teams={data.teams} />*/}
                <img src={logo} alt="Business Esbjerg" className="h-9 w-auto self-start"/>
                <p>Holiday Planner</p>
            </SidebarHeader>
            <SidebarContent className="gap-4 px-2">
                {hasRole(user?.role, Roles.Employee) && <NavEmployee items={employeeItems}/>}
                {hasRole(user?.role, Roles.Accountant) && <NavAccounting items={accountingItems}/>}
                {hasRole(user?.role, Roles.Admin) && <NavAdmin items={adminItems}/>}
            </SidebarContent>
            <SidebarFooter>
                <div className="hidden flex-col gap-[18px] md:flex">
                    <NavLink to="/help" className="flex items-center gap-2.5 px-3 text-[14px] leading-normal text-sidebar-foreground font-medium hover:text-ink">
                        <CircleHelp className="size-[18px] shrink-0" aria-hidden />
                        Help &amp; company policy
                    </NavLink>
                    <div className="h-px bg-line" />
                </div>
                <SidebarSeparator className="h-px bg-sidebar-foreground!"/>
                <NavUser user={{name: user?.name!, email: user?.email!}} />
            </SidebarFooter>
            <SidebarRail />
        </Sidebar>
    )
}