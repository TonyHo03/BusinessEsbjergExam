import { serve } from "bun";
import index from "./index.html";

const server = serve({
  routes: {
    // Alle adresser leverer React-appen, og React Router finder den rigtige side
    "/*": index,
  },

  development: process.env.NODE_ENV !== "production" && {
    hmr: true,
    console: true,
  },
});

console.log(`🚀 Server running at ${server.url}`);