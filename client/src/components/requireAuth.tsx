import {Navigate, Outlet, useLocation} from "react-router";
import {useAuth} from "@/components/authProvider.tsx";

export function RequireAuth() {

    const { user, loading } = useAuth();
    const location = useLocation();


    if (loading) return <div className="p-8">Loading...</div>;


    if (!user) {
        return <Navigate to="/login" replace state={{ from: location }} />;
    }

    return (
        <Outlet/>
    );

}