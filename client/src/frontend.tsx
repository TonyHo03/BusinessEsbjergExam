/**
 * This file is the entry point for the React app, it sets up the root
 * element and renders the App component to the DOM.
 *
 * It is included in `src/index.html`.
 */

import { StrictMode } from "react";
import { createRoot, type Root } from "react-dom/client";
import { App } from "./App";
import {BrowserRouter} from "react-router";
import {TooltipProvider} from "./components/ui/tooltip.tsx";
import {AuthProvider} from "./components/authProvider.tsx";

const elem = document.getElementById("root")!;
const app = (
  <StrictMode>
      <BrowserRouter>
          <AuthProvider>
              <TooltipProvider>
                  <App />
              </TooltipProvider>
          </AuthProvider>
      </BrowserRouter>
  </StrictMode>
);

// https://bun.com/docs/bundler/hot-reloading#import-meta-hot-data
type HotData = { root?: Root };
((import.meta.hot.data as HotData).root ??= createRoot(elem)).render(app);
