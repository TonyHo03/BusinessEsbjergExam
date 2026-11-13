import { NavLink, Outlet } from "react-router";

type NavItem = { to: string; label: string; end?: boolean };
type NavSection = { title: string; items: NavItem[] };

// Senere (efter login, #10) vises kun de sektioner, brugerens rolle har adgang til
const sections: NavSection[] = [
    {
        title: "Employee",
        items: [
            { to: "/", label: "Dashboard", end: true },
            { to: "/requests/new", label: "Request holiday" },
            { to: "/requests", label: "My requests", end: true },
        ],
    },
    {
        title: "Accounting",
        items: [
            { to: "/approvals", label: "Pending approvals" },
            { to: "/employees", label: "Employee overview" },
        ],
    },
    {
        title: "Admin",
        items: [
            { to: "/admin/parameters", label: "Parameters" },
            { to: "/admin/users", label: "User management" },
        ],
    },
];

export function Layout() {
    return (
        <div className="layout">
            <aside className="sidebar">
                <div className="brand">
                    <span className="brand-mark">be</span>
                    <span className="brand-name">
            Business Esbjerg
            <small>Holiday Planner</small>
          </span>
                </div>

                <nav aria-label="Main navigation">
                    {sections.map((section) => (
                        <div key={section.title} className="nav-section">
                            <p className="nav-title">{section.title}</p>
                            {section.items.map((item) => (
                                <NavLink
                                    key={item.to}
                                    to={item.to}
                                    end={item.end}
                                    className={({ isActive }) => (isActive ? "nav-link active" : "nav-link")}
                                >
                                    {item.label}
                                </NavLink>
                            ))}
                        </div>
                    ))}
                </nav>
            </aside>

            <main className="content">
                <Outlet />
            </main>
        </div>
    );
}