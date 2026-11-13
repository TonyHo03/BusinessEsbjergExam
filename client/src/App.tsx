import "./index.css";
import {Route, Routes} from "react-router";
import {LoginView} from "./pages/login.tsx";
import { PendingApprovalsPage } from "./pages/PendingApprovalsPage";


export function App() {
  return (
      <div className="flex justify-center items-center w-full h-screen">
          <Routes>
              <Route path={"/login"} element={<LoginView/>}/>
              <Route path={"/approvals"} element={<PendingApprovalsPage/>}/>
          </Routes>
      </div>
  );
}

export default App;
