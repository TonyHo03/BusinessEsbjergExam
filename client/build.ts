import tailwind from "bun-plugin-tailwind";

// Produktionsbuild. Tailwind-pluginnet skal med her, fordi `bun build` på kommandolinjen ikke kan bruge plugins.
const result = await Bun.build({
    entrypoints: ["./src/index.html"],
    outdir: "./dist",
    target: "browser",
    minify: true,
    sourcemap: "linked",
    plugins: [tailwind],
    define: { "process.env.NODE_ENV": JSON.stringify("production") },
    env: "BUN_PUBLIC_*",
});

if (!result.success) {
    for (const log of result.logs) console.error(log);
    process.exit(1);
}

console.log(`Built ${result.outputs.length} files to ./dist`);