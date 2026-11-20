import {expect, test} from "@playwright/test";
import {login, users} from "./helpers";

test("an employee logs in and sees their dashboard", async ({page}) => {
    await login(page, users.mette);

    await expect(page.getByText("Hi, Mette")).toBeVisible();
});

test("a wrong password does not log you in", async ({page}) => {
    await page.goto("/login");
    await page.locator('input[type="email"]').fill(users.mette.email);
    await page.locator('input[type="password"]').fill("wrong-password");

    const response = page.waitForResponse(r => /\/api\/auth\/login/i.test(r.url()));
    await page.getByRole("button", {name: "Sign In"}).click();

    expect((await response).ok()).toBe(false);
    await expect(page).toHaveURL(/\/login/);
});

test("you are sent to login when not logged in", async ({page}) => {
    await page.context().clearCookies();
    await page.goto("/dashboard");

    await expect(page).toHaveURL(/\/login/);
});

test("an employee cannot open admin pages", async ({page}) => {
    await login(page, users.mette);

    await expect(page.getByRole("link", {name: "User Management"})).toHaveCount(0);

    await page.goto("/usermanagement");
    await expect(page).toHaveURL(/\/dashboard/);
});