import {defineConfig, devices} from "@playwright/test";

// E2E-testene kører mod den rigtige frontend, API og database.
// Databasen skal køre først: docker compose up -d
// API og frontend startes automatisk, hvis de ikke allerede kører.
export default defineConfig({
    testDir: "./tests",
    // Alle tests deler samme database, så de køres én ad gangen
    workers: 1,
    fullyParallel: false,
    reporter: "list",
    use: {
        baseURL: "http://localhost:3000",
        trace: "retain-on-failure",
        screenshot: "only-on-failure",
    },
    projects: [{name: "chromium", use: {...devices["Desktop Chrome"]}}],
    webServer: [
        {
            command: "dotnet run --project ../server/API --launch-profile http",
            url: "http://localhost:5265/swagger/index.html",
            reuseExistingServer: true,
            timeout: 120_000,
        },
        {
            command: "bun dev",
            cwd: "../client",
            url: "http://localhost:3000",
            reuseExistingServer: true,
            timeout: 60_000,
            stderr: "ignore",
        },
    ],
});