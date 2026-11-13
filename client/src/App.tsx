import "./index.css";
import {Route, Routes} from "react-router";
import {LoginView} from "./pages/login.tsx";

export function App() {
  return (
      <div className="flex justify-center items-center w-full h-screen">
          <Routes>
              <Route path={"/login"} element={<LoginView/>}/>
          </Routes>
      </div>
  );
}

export default App;
