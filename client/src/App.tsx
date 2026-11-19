import "./index.css";
import {Route, Routes} from "react-router";
import {LoginView} from "./pages/login.tsx";
import { PendingApprovalsPage } from "./pages/PendingApprovalsPage";
import {AppLayout} from "./components/appLayout.tsx";
import {UserManagementView} from "./pages/usermanagement.tsx";
import {HelpPage} from "./pages/helpandpolicy.tsx";
import {routes} from "@/components/app-sidebar.tsx";
import {RequireAuth} from "./components/requireAuth.tsx";
import RequireRole from "./components/RequireRole.tsx";
import {Roles} from "@/api/Api.ts";
import {RequestHolidayView} from "./pages/requestHoliday.tsx";
import {ParametersView} from "@/pages/parameters.tsx";
import {EmployeeOverviewView} from "@/pages/employeeOverview.tsx";
import {MyRequestsView} from "@/pages/myRequests.tsx";

export function App() {
    return (
        <div className="flex justify-center items-center w-full h-screen bg-background">
            <Routes>
                <Route path={"/login"} element={<LoginView/>}/>

                <Route element={<RequireAuth/>}>
                    <Route element={<AppLayout/>}>
                        <Route path={routes.dashboard}/>
                        <Route path={routes.requestHolidays} element={<RequestHolidayView/>}/>
                        <Route path={routes.myRequests} element={<MyRequestsView/>}/>
                        <Route path={routes.helpAndPolicies} element={<HelpPage/>}/>

                        {/*<Route element={<RequireRole min={Roles.Accountant}/>}>
                            <Route path={routes.pendingApprovals} element={<PendingApprovalsPage/>}/>
                        </Route>

                        <Route element={<RequireRole min={Roles.Admin}/>}>
                            <Route path={routes.userManagement} element={<UserManagementView/>}/>
                        </Route>*/}

                        <Route path={routes.pendingApprovals} element={<PendingApprovalsPage/>}/>
                        <Route path={routes.employeeOverview} element={<EmployeeOverviewView/>}/>
                        <Route path={routes.userManagement} element={<UserManagementView/>}/>
                        <Route path={routes.parameters} element={<ParametersView/>}/>

                    </Route>
                </Route>
            </Routes>
        </div>
    );
}

export default App;
