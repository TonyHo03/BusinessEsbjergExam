import { serve } from "bun";
import index from "./index.html";

const server = serve({
  routes: {
    // Svar til søgemaskiner. Uden denne route svarer serveren med index.html
    "/robots.txt": new Response("User-agent: *\nAllow: /\n", {
      headers: { "Content-Type": "text/plain; charset=utf-8" },
    }),

    // Alle andre adresser leverer React-appen, og React Router finder den rigtige side
    "/*": index,
  },

  development: process.env.NODE_ENV !== "production" && {
    hmr: true,
    console: true,
  },
});

console.log(`🚀 Server running at ${server.url}`);