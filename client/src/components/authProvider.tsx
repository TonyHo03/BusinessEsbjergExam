// auth/AuthProvider.tsx
import {createContext, type ReactNode, useContext, useEffect, useState} from "react";
import {api} from "../api/client.ts";
import {type CurrentLoggedUser, Roles} from "../api/Api.ts";

type AuthContextValue = {
    user: CurrentLoggedUser | null;
    loading: boolean;
    refresh: () => Promise<void>;
    logout: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
    const [user, setUser] = useState<CurrentLoggedUser | null>(null);
    const [loading, setLoading] = useState(true);

    const refresh = async () => {
        try {
            const me = await api.api.authMe();
            setUser(me);
        } catch (err) {
            console.error("refresh failed:", err);
            setUser(null);
        }
    };

    const logout = () => {
        localStorage.removeItem("token");
        setUser(null);
    };

    useEffect(() => {
        refresh().finally(() => setLoading(false));
    }, []);

    return (
        <AuthContext value={{ user, loading, refresh, logout }}>
            {children}
        </AuthContext>
    );
}

export function useAuth() {
    const ctx = useContext(AuthContext);
    if (!ctx) throw new Error("useAuth must be used inside AuthProvider");
    return ctx;
}