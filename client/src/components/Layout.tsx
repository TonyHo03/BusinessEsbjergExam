import { Outlet } from "react-router";
import { Sidebar } from "./Sidebar";
import { Topbar } from "./Topbar";

export function Layout() {
    return (
        <div className="min-h-screen md:flex">
            <Sidebar />

            <div className="flex min-w-0 flex-1 flex-col">
                <Topbar />

                <main className="flex flex-1 flex-col gap-7 px-6 pt-9 pb-8 md:px-10">
                    <div className="flex-1">
                        <Outlet />
                    </div>

                    <footer className="flex flex-wrap justify-between gap-2 pt-1 text-[11px] text-subtle">
                        <span>Business Esbjerg · Internal holiday planning</span>
                        <span>All dates use Danish local time</span>
                    </footer>
                </main>
            </div>
        </div>
    );
}