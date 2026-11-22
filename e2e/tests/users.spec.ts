import {expect, test} from "@playwright/test";
import {login, users} from "./helpers";

test("admin creates an employee and is asked for a work schedule right away", async ({page}) => {
    const id = Date.now();
    const name = `E2E Employee ${id}`;
    const email = `e2e${id}@businessesbjerg.com`;

    await login(page, users.admin);
    await page.goto("/usermanagement");

    // Opret medarbejderen
    await page.getByRole("button", {name: "Add employee"}).click();
    const create = page.getByRole("dialog");
    await create.getByLabel("Name", {exact: true}).fill(name);
    await create.getByLabel("Email", {exact: true}).fill(email);
    await create.getByLabel("Temporary password").fill("Password123");
    await create.getByRole("button", {name: "Create"}).click();

    // Skemadialogen åbner af sig selv, fordi en ny medarbejder skal have et skema
    const schedule = page.getByRole("dialog");
    await expect(schedule).toContainText(`Work schedule – ${name}`);
    await expect(schedule).toContainText("has no work schedule yet");

    await schedule.getByRole("button", {name: "Save schedule"}).click();
    await expect(schedule.getByText("Current")).toBeVisible();

    await page.keyboard.press("Escape");
    await expect(schedule).toBeHidden();

    // Den nye medarbejder står i listen
    await page.getByLabel("Find a user:").fill(name);
    const row = page.getByRole("row").filter({hasText: email});
    await expect(row).toContainText("Employee");
    await expect(row).toContainText("Active");
});