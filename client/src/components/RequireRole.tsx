import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "@/components/authProvider";
import { hasRole} from "@/lib/utils.ts";
import type {Roles} from "@/api/Api.ts";
import {useNavigate} from "react-router";
import {useEffect} from "react";
import * as React from "react";

export default function RequireRole({ min, children }: { min: Roles, children: React.ReactNode }) {
    const { user, loading } = useAuth();
    const navigate = useNavigate();

    useEffect(() => {
        // Vent med at vurdere før loade-processen er færdig
        if (loading) return;

        if (!user || !hasRole(user.role, min)) {
            navigate("/dashboard", { replace: true });
        }
    }, [user, loading, min, navigate]);

    // Vis en loader eller ingenting, imens auth-data hentes
    if (loading) {
        return <div>Indlæser...</div>; // Eller din egen loading-spinner
    }

    if (!user || !hasRole(user.role, min)) {
        return null; // Undgå at vise indholdet mens navigate udføres
    }

    return <Outlet />;
}