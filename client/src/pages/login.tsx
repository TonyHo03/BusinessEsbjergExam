import {Label} from "../components/ui/label.tsx";


export function LoginView() {
    return (
        <div className="flex w-screen h-full bg-transparent">
            <div className="flex w-3/7 h-full bg-[#ce6b4c]">

            </div>
            <div className="flex flex-col justify-center items-center w-4/7 h-full bg-[#FFFAF0]">
                <div className="flex flex-col w-125">
                    <Label className="text-4xl font-light">Welcome back!</Label>
                </div>
            </div>
        </div>
    );
}