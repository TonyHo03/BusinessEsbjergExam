import {expect, test} from "@playwright/test";
import {API, login, randomMondayAhead, users} from "./helpers";

const METTE_ID = "11111111-1111-1111-1111-111111111111";

test("holiday is approved, the employee asks to cancel it, and accounting approves the cancellation", async ({page}) => {
    const comment = `E2E ${Date.now()}`;
    const monday = randomMondayAhead();

    // Mette søger én dag fri. Request holiday-siden er ikke færdig, så ansøgningen sendes direkte til API'et
    await login(page, users.mette);
    const submit = await page.request.post(`${API}/api/requests`, {
        data: {employeeId: METTE_ID, account: "Weeks1To5", startDate: monday, endDate: monday, comment},
    });
    expect(submit.status(), await submit.text()).toBe(201);

    // Bogholderiet godkender
    await login(page, users.admin);
    await page.goto("/approvals");
    const pending = page.getByRole("row").filter({hasText: comment});
    await pending.getByRole("button", {name: "Approve request from Mette Jensen"}).click();
    await expect(pending).toHaveCount(0);

    // Mette ser ferien som godkendt og beder om at få den annulleret
    await login(page, users.mette);
    await page.goto("/my-requests");
    const mine = page.getByRole("row").filter({hasText: comment});
    await expect(mine).toContainText("Approved");
    await mine.getByRole("button", {name: "Request cancellation"}).click();
    await expect(mine).toContainText("Cancellation requested");

    // Bogholderiet ser annulleringen og godkender den
    await login(page, users.admin);
    await page.goto("/approvals");
    const cancellation = page.getByRole("row").filter({hasText: comment});
    await expect(cancellation).toContainText("Wants to cancel this approved holiday");
    await cancellation.getByRole("button", {name: "Approve cancellation from Mette Jensen"}).click();
    await expect(cancellation).toHaveCount(0);

    // Ferien står nu som annulleret hos Mette
    await login(page, users.mette);
    await page.goto("/my-requests");
    await expect(page.getByRole("row").filter({hasText: comment})).toContainText("Cancelled");
});