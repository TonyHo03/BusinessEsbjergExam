import { Outlet, useNavigate } from "react-router";
import { useEffect } from "react";
import { useAuth } from "@/components/authProvider";
import { hasRole } from "@/lib/utils.ts";
import type { Roles } from "@/api/Api.ts";

/**
 * Viser kun siderne under sig, hvis brugeren har mindst rollen `min`.
 * Ellers sendes brugeren til dashboard. Det er kun for brugervenlighed –
 * det er backend, der rent faktisk beskytter data.
 */
export default function RequireRole({ min }: { min: Roles }) {
    const { user, loading } = useAuth();
    const navigate = useNavigate();
    const allowed = !!user && hasRole(user.role, min);

    useEffect(() => {
        if (!loading && !allowed) navigate("/dashboard", { replace: true });
    }, [loading, allowed, navigate]);

    if (loading || !allowed) return null;

    return <Outlet />;
}