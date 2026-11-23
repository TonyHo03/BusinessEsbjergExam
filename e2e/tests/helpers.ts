import {expect, type Page} from "@playwright/test";

export const API = "http://localhost:5265";

// Brugerne findes i udviklingsdatabasen: admin fra appsettings.Development.json, Mette fra init.sql
export const users = {
    admin: {email: "admin@businessesbjerg.com", password: "admin123"},
    mette: {email: "mette@businessesbjerg.com", password: "mette123"},
};

type User = {email: string, password: string};

export async function login(page: Page, user: User) {
    await page.context().clearCookies();
    await page.goto("/login");
    await page.locator('input[type="email"]').fill(user.email);
    await page.locator('input[type="password"]').fill(user.password);
    await page.getByRole("button", {name: "Sign In"}).click();
    await expect(page).not.toHaveURL(/\/login/);
}

// En tilfældig mandag 6–40 uger ude, så varslet er overholdt, og to kørsler ikke rammer samme dag
export function randomMondayAhead(): string {
    const date = new Date();
    date.setDate(date.getDate() + 7 * (6 + Math.floor(Math.random() * 35)));
    while (date.getDay() !== 1) date.setDate(date.getDate() + 1);
    return date.toLocaleDateString("sv-SE"); // YYYY-MM-DD i lokal tid
}