import {Outlet} from "react-router";
import {Sidebar, SidebarContent, SidebarFooter, SidebarHeader, SidebarProvider} from "./ui/sidebar.tsx";
import {AppSidebar} from "./app-sidebar.tsx";
import {Topbar} from "@/components/topbar.tsx";


export function AppLayout() {

    return (
        <div className="flex w-full h-full">
            <SidebarProvider>
                <AppSidebar/>
                <main className="flex flex-col flex-1 overflow-y-auto">
                    <Topbar/>
                    <Outlet/>
                </main>
            </SidebarProvider>
        </div>
    );
}