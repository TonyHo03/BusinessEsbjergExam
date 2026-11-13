import { Api } from "./Api";

// Én fælles klient, som alle sider bruger
export const api = new Api();

// Midlertidigt: Mettes id, indtil login (#10) er på plads.
// Derefter skal id'et komme fra den indloggede bruger.
export const CURRENT_EMPLOYEE_ID = "11111111-1111-1111-1111-111111111111";