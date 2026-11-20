import {Label} from "../components/ui/label.tsx";
import {Input} from "../components/ui/input.tsx";
import {ArrowRight} from "lucide-react";
import {type FormEvent, useEffect, useState} from "react";
import {api} from "../api/client.ts";
import {useAuth} from "@/components/authProvider.tsx";
import {useNavigate} from "react-router";


export function LoginView() {

    const [email, setEmail] = useState<string>("");
    const [password, setPassword] = useState<string>("");
    const {user, refresh} = useAuth();
    const navigate = useNavigate();

    // Skift til dashboard, når login er lykkedes
    useEffect(() => {
        if (user) navigate("/dashboard");
    }, [user]);

    const handleLogin = async (e: FormEvent) => {
        e.preventDefault(); // undgå at siden genindlæses
        try {
            await api.api.authLogin({email: email, password: password});
            await refresh();
        }
        catch (err) {
            console.log(err);
        }
    }

    return (
        <div className="flex w-screen h-full bg-transparent">
            <div className="flex w-3/7 h-full bg-[#ce6b4c]">

            </div>

            <div className="flex flex-col justify-center items-center w-4/7 h-full bg-[#FFFAF0]">
                <form onSubmit={handleLogin} className="flex flex-col w-125 gap-12">
                    <div className="flex flex-col w-full gap-4">
                        <Label className="text-4xl text-[#0F2338] font-normal">Welcome back</Label>
                        <Label className="text-xl text-[#516080]">Sign in with your email</Label>
                    </div>
                    <div className="flex flex-col w-full gap-4">
                        <div className="flex flex-col w-full gap-2">
                            <Label className="text-[#0F2338] font-bold">Work email</Label>
                            <Input type="email" value={email} onChange={e => setEmail(e.target.value)} className="border-2 border-[#EBE3D5] h-10 focus-visible:ring-[#ce6b4c] text-[#0F2338] transition-all"/>
                        </div>
                        <div className="flex flex-col w-full gap-2">
                            <Label className="text-[#0F2338] font-bold">Password</Label>
                            <Input type="password" value={password} onChange={e => setPassword(e.target.value)} className="border-2 border-[#EBE3D5] h-10 focus-visible:ring-[#ce6b4c] text-[#0F2338] transition-all"/>
                        </div>
                    </div>
                    <button type="submit" className="flex justify-center items-center w-full h-10 bg-[#ce6b4c] font-semibold text-sm rounded-lg gap-3"><ArrowRight/>Sign In</button>
                </form>
            </div>
        </div>
    );
}