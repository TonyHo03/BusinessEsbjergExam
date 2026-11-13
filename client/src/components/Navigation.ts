import {
    CalendarPlus,
    CircleCheck,
    CircleHelp,
    LayoutDashboard,
    ListChecks,
    SlidersHorizontal,
    UserCog,
    Users,
    type LucideIcon,
} from "lucide-react";

export type Workspace = "Employee" | "Accounting" | "Admin";

export type NavItem = {
    to: string;
    label: string;
    icon: LucideIcon;
    end?: boolean;
    /** Tal i en lille mærkat, fx antal ventende ansøgninger */
    badge?: number;
};
export type NavSection = { workspace: Workspace; items: NavItem[] };

// Senere (efter login, #10) vises kun de sektioner, brugerens rolle har adgang til
export const sections: NavSection[] = [
    {
        workspace: "Employee",
        items: [
            { to: "/", label: "Dashboard", icon: LayoutDashboard, end: true },
            { to: "/requests/new", label: "Request holiday", icon: CalendarPlus },
            { to: "/requests", label: "My requests", icon: ListChecks, end: true },
        ],
    },
    {
        workspace: "Accounting",
        items: [
            { to: "/approvals", label: "Pending approvals", icon: CircleCheck },
            { to: "/employees", label: "Employee overview", icon: Users },
        ],
    },
    {
        workspace: "Admin",
        items: [
            { to: "/admin/parameters", label: "Parameters", icon: SlidersHorizontal },
            { to: "/admin/users", label: "User management", icon: UserCog },
        ],
    },
];

// Sider der ikke ligger i en menusektion, men stadig skal have en titel i topbaren
const otherPages: NavItem[] = [{ to: "/help", label: "Help & company policy", icon: CircleHelp }];

/** Finder det menupunkt og den sektion, der passer til den aktuelle adresse. */
export function findCurrent(pathname: string): { item?: NavItem; workspace: Workspace } {
    for (const section of sections) {
        const item = section.items.find((i) => (i.end ? pathname === i.to : pathname.startsWith(i.to)));
        if (item) return { item, workspace: section.workspace };
    }

    const other = otherPages.find((p) => pathname === p.to);
    return { item: other, workspace: "Employee" };
}