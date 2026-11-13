import { BrowserRouter, Route, Routes } from "react-router";
import { Layout } from "./components/Layout";
import { DashboardPage } from "./pages/DashboardPage";
import { RequestHolidayPage } from "./pages/RequestHolidayPage";
import { MyRequestsPage } from "./pages/MyRequestsPage";
import { ApprovalsPage } from "./pages/ApprovalsPage";
import { EmployeeOverviewPage } from "./pages/EmployeeOverviewPage";
import { ParametersPage } from "./pages/ParametersPage";
import { UsersPage } from "./pages/UsersPage";
import { HelpPage } from "./pages/HelpPage";
import { NotFoundPage } from "./pages/NotFoundPage";

export function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route element={<Layout />}>
                    {/* Medarbejder */}
                    <Route index element={<DashboardPage />} />
                    <Route path="requests" element={<MyRequestsPage />} />
                    <Route path="requests/new" element={<RequestHolidayPage />} />

                    {/* Bogholderi */}
                    <Route path="approvals" element={<ApprovalsPage />} />
                    <Route path="employees" element={<EmployeeOverviewPage />} />

                    {/* Admin */}
                    <Route path="admin/parameters" element={<ParametersPage />} />
                    <Route path="admin/users" element={<UsersPage />} />

                    {/* Fælles */}
                    <Route path="help" element={<HelpPage />} />

                    <Route path="*" element={<NotFoundPage />} />
                </Route>
            </Routes>
        </BrowserRouter>
    );
}

export default App;