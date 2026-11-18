import {Api} from "./Api.ts";

export const api = new Api({baseUrl: "http://localhost:5265", baseApiParams: {credentials: "include"}});