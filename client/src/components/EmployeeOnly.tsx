import {Outlet, useNavigate} from "react-router";
import {useEffect} from "react";
import {useAuth} from "@/components/authProvider";
import {Roles} from "@/api/Api.ts";
import {routes} from "@/components/app-sidebar.tsx";

// Accountant- og system owner-kontiene har ingen ferie, så de sendes til deres egen startside
export default function EmployeeOnly() {
    const {user, loading} = useAuth();
    const navigate = useNavigate();

    const home = user?.role === Roles.Admin ? routes.userManagement
        : user?.role === Roles.Accountant ? routes.pendingApprovals
            : null;

    useEffect(() => {
        if (!loading && home) navigate(home, {replace: true});
    }, [loading, home, navigate]);

    if (loading || home) return null;

    return <Outlet/>;
}