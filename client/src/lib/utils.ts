import {Roles} from "@/api/Api.ts";

export { cn } from "cn"

export const roleRank = {Employee: 1, Accountant: 2, Admin: 3}

export const hasRole = (role: Roles | undefined, min: Roles) => {
    return role !== undefined && roleRank[role] >= roleRank[min];
}