/* eslint-disable */
/* tslint:disable */
// @ts-nocheck
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

export enum IssueSeverity {
  Error = "Error",
  Warning = "Warning",
}

export enum IssueCode {
  NoWorkingDays = "NoWorkingDays",
  StartInPast = "StartInPast",
  NotEnoughNotice = "NotEnoughNotice",
  InsufficientBalance = "InsufficientBalance",
  UsesHolidayDebt = "UsesHolidayDebt",
  NegativeOn31August = "NegativeOn31August",
  ShortBlock = "ShortBlock",
  OverlapsExistingRequest = "OverlapsExistingRequest",
}

export enum DayKind {
  Workday = "Workday",
  NonWorkingDay = "NonWorkingDay",
  DayOff = "DayOff",
}

export enum ParameterVersionStatus {
  Past = "Past",
  Current = "Current",
  Upcoming = "Upcoming",
}

export enum ParameterKind {
  Decimal = "Decimal",
  Integer = "Integer",
  Boolean = "Boolean",
}

export enum Status {
  Active = "Active",
  Inactive = "Inactive",
}

export enum ExpiryStatus {
  NotYetUsable = "NotYetUsable",
  Active = "Active",
  ExpiringSoon = "ExpiringSoon",
  Expired = "Expired",
}

export enum Roles {
  Employee = "Employee",
  Accountant = "Accountant",
  Admin = "Admin",
}

export enum RequestStatus {
  Pending = "Pending",
  Approved = "Approved",
  Rejected = "Rejected",
  Cancelled = "Cancelled",
  CancellationRequested = "CancellationRequested",
}

export enum HolidayAccount {
  Weeks1To5 = "Weeks1To5",
  Week6 = "Week6",
}

export interface PendingRequestDto {
  /** @format guid */
  requestId?: string;
  /** @format guid */
  employeeId?: string;
  employeeName?: string;
  /** @format date */
  startDate?: string;
  /** @format date */
  endDate?: string;
  /** @format int32 */
  workingDays?: number;
  /** @format decimal */
  hours?: number;
  account?: HolidayAccount;
  /** @format date-time */
  submittedAt?: string;
  note?: string | null;
  isOwnRequest?: boolean;
  isCancellation?: boolean;
}

export interface DecisionResponseDto {
  /** @format guid */
  requestId?: string;
  status?: RequestStatus;
}

export interface RejectRequestDto {
  reason?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface CurrentLoggedUser {
  /** @format guid */
  id?: string;
  name?: string;
  email?: string;
  role?: Roles;
}

export interface BalancesResponse {
  /** @format date */
  asOf?: string;
  /** @format decimal */
  dayLength?: number;
  /** @format decimal */
  availableWeeks1To5?: number;
  /** @format decimal */
  availableWeek6?: number;
  /** @format decimal */
  pendingWeeks1To5?: number;
  /** @format decimal */
  pendingWeek6?: number;
  periods?: BalanceDto[];
}

export interface BalanceDto {
  account?: HolidayAccount;
  /** @format int32 */
  earningYear?: number;
  /** @format decimal */
  accrued?: number;
  /** @format decimal */
  used?: number;
  /** @format decimal */
  remaining?: number;
  /** @format decimal */
  remainingDays?: number;
  /** @format date */
  usableFrom?: string;
  /** @format date */
  expiresOn?: string;
  status?: ExpiryStatus;
  needsAction?: boolean;
}

export interface EmployeeOverviewRow {
  /** @format guid */
  employeeId?: string;
  name?: string;
  email?: string;
  role?: Roles;
  status?: Status;
  hasSchedule?: boolean;
  /** @format decimal */
  dayLength?: number | null;
  /** @format decimal */
  availableWeeks1To5?: number | null;
  /** @format decimal */
  availableWeek6?: number | null;
  /** @format decimal */
  pendingHours?: number;
  /** @format decimal */
  expiringHours?: number | null;
  /** @format date */
  expiresOn?: string | null;
}

export interface ParameterDto {
  key?: string;
  description?: string;
  kind?: ParameterKind;
  currentValue?: string | null;
  /** @format date */
  currentFrom?: string | null;
  versions?: ParameterVersionDto[];
}

export interface ParameterVersionDto {
  /** @format guid */
  id?: string;
  value?: string;
  /** @format date */
  effectiveFrom?: string;
  status?: ParameterVersionStatus;
}

export interface ParameterInput {
  key?: string;
  value?: string;
  /** @format date */
  effectiveFrom?: string;
}

export interface RequestPreview {
  days?: RequestDay[];
  /** @format decimal */
  totalHours?: number;
  /** @format int32 */
  workingDays?: number;
  isMainHoliday?: boolean;
  isValid?: boolean;
  issues?: ValidationIssue[];
}

export interface RequestDay {
  /** @format date */
  date?: string;
  kind?: DayKind;
  /** @format decimal */
  hours?: number;
}

export interface ValidationIssue {
  code?: IssueCode;
  severity?: IssueSeverity;
  message?: string;
}

export interface ProblemDetails {
  type?: string | null;
  title?: string | null;
  /** @format int32 */
  status?: number | null;
  detail?: string | null;
  instance?: string | null;
  [key: string]: any;
}

export interface RequestInput {
  /** @format guid */
  employeeId?: string;
  account?: HolidayAccount;
  /** @format date */
  startDate?: string;
  /** @format date */
  endDate?: string;
  comment?: string | null;
}

export interface RequestDto {
  /** @format guid */
  id?: string;
  account?: HolidayAccount;
  /** @format date */
  startDate?: string;
  /** @format date */
  endDate?: string;
  /** @format decimal */
  hours?: number;
  status?: RequestStatus;
  comment?: string | null;
  rejectionReason?: string | null;
  /** @format date-time */
  createdAt?: string;
}

export interface TeamCalendarResponse {
  /** @format date */
  from?: string;
  /** @format date */
  to?: string;
  members?: TeamMember[];
  days?: TeamDay[];
}

export interface TeamMember {
  /** @format guid */
  employeeId?: string;
  name?: string;
  absences?: TeamAbsence[];
}

export interface TeamAbsence {
  /** @format date */
  startDate?: string;
  /** @format date */
  endDate?: string;
}

export interface TeamDay {
  /** @format date */
  date?: string;
  isWorkingDay?: boolean;
  /** @format int32 */
  absent?: number;
  /** @format int32 */
  present?: number;
}

export interface TransferDto {
  /** @format guid */
  id?: string;
  /** @format guid */
  employeeId?: string;
  /** @format int32 */
  fromEarningYear?: number;
  /** @format decimal */
  days?: number;
  status?: RequestStatus;
  comment?: string | null;
  rejectionReason?: string | null;
  /** @format date-time */
  createdAt?: string;
}

export interface TransferInput {
  /** @format guid */
  employeeId?: string;
  /** @format int32 */
  fromEarningYear?: number;
  /** @format decimal */
  days?: number;
  comment?: string | null;
}

export interface PendingTransferDto {
  /** @format guid */
  id?: string;
  /** @format guid */
  employeeId?: string;
  employeeName?: string;
  /** @format int32 */
  fromEarningYear?: number;
  /** @format decimal */
  days?: number;
  comment?: string | null;
  /** @format date-time */
  createdAt?: string;
}

export interface EmployeeDto {
  /** @format guid */
  id?: string;
  name?: string;
  email?: string;
  status?: Status;
  role?: Roles;
}

export interface CreateEmployeeRequest {
  password?: string;
  name?: string;
  email?: string;
  /** @format date */
  startDate?: string;
  /** @format date */
  endDate?: string | null;
  role?: Roles;
}

export interface UpdateEmployeeRequest {
  name?: string;
  email?: string;
  /** @format date */
  startDate?: string;
  /** @format date */
  endDate?: string | null;
  role?: Roles;
}

export interface WorkSchedulesResponse {
  /** @format date */
  employeeStartDate?: string;
  schedules?: WorkScheduleDto[];
}

export interface WorkScheduleDto {
  /** @format guid */
  id?: string;
  /** @format date */
  effectiveFrom?: string;
  /** @format date */
  effectiveTo?: string | null;
  monday?: DayTimes;
  tuesday?: DayTimes;
  wednesday?: DayTimes;
  thursday?: DayTimes;
  friday?: DayTimes;
  /** @format decimal */
  weeklyHours?: number;
  isCurrent?: boolean;
}

export interface DayTimes {
  start?: string | null;
  end?: string | null;
}

export interface WorkScheduleInput {
  /** @format date */
  effectiveFrom?: string;
  monday?: DayTimes;
  tuesday?: DayTimes;
  wednesday?: DayTimes;
  thursday?: DayTimes;
  friday?: DayTimes;
}

export interface ApprovalsApproveParams {
  /** @format guid */
  id: string;
}

export interface ApprovalsRejectParams {
  /** @format guid */
  id: string;
}

export interface ApprovalsCancelParams {
  /** @format guid */
  id: string;
}

export interface ApprovalsRequestCancellationParams {
  /** @format guid */
  id: string;
}

export interface BalancesGetParams {
  /** @format date */
  asOf?: string | null;
  /** @format guid */
  employeeId: string;
}

export interface EmployeeOverviewGetParams {
  /** @format date */
  asOf?: string | null;
}

export interface ParametersGetParams {
  /** @format date */
  asOf?: string | null;
}

export interface ParametersDeleteParams {
  /** @format guid */
  id: string;
}

export interface RequestsGetForEmployeeParams {
  /** @format guid */
  employeeId: string;
}

export interface TeamHolidaysGetParams {
  /** @format date */
  from?: string;
  /** @format date */
  to?: string;
}

export interface TransfersGetForEmployeeParams {
  /** @format guid */
  employeeId: string;
}

export interface TransfersApproveParams {
  /** @format guid */
  id: string;
}

export interface TransfersRejectParams {
  /** @format guid */
  id: string;
}

export interface UserGetUserByEmailParams {
  email: string;
}

export interface UserUpdateUserParams {
  /** @format guid */
  id: string;
}

export interface UserActivateUserParams {
  /** @format guid */
  id: string;
}

export interface UserDeactivateUserParams {
  /** @format guid */
  id: string;
}

export interface WorkSchedulesGetParams {
  /** @format guid */
  employeeId: string;
}

export interface WorkSchedulesAddParams {
  /** @format guid */
  employeeId: string;
}

export interface WorkSchedulesDeleteParams {
  /** @format guid */
  employeeId: string;
  /** @format guid */
  id: string;
}

export type QueryParamsType = Record<string | number, any>;
export type ResponseFormat = keyof Omit<Body, "body" | "bodyUsed">;

export interface FullRequestParams extends Omit<RequestInit, "body"> {
  /** set parameter to `true` for call `securityWorker` for this request */
  secure?: boolean;
  /** request path */
  path: string;
  /** content type of request body */
  type?: ContentType;
  /** query params */
  query?: QueryParamsType;
  /** format of response (i.e. response.json() -> format: "json") */
  format?: ResponseFormat;
  /** request body */
  body?: unknown;
  /** base url */
  baseUrl?: string;
  /** request cancellation token */
  cancelToken?: CancelToken;
}

export type RequestParams = Omit<
  FullRequestParams,
  "body" | "method" | "query" | "path"
>;

export interface ApiConfig<SecurityDataType = unknown> {
  baseUrl?: string;
  baseApiParams?: Omit<RequestParams, "baseUrl" | "cancelToken" | "signal">;
  securityWorker?: (
    securityData: SecurityDataType | null,
  ) => Promise<RequestParams | void> | RequestParams | void;
  customFetch?: typeof fetch;
}

export interface HttpResponse<D extends unknown, E extends unknown = unknown>
  extends Response {
  data: D;
  error: E;
}

type CancelToken = Symbol | string | number;

export enum ContentType {
  Json = "application/json",
  JsonApi = "application/vnd.api+json",
  FormData = "multipart/form-data",
  UrlEncoded = "application/x-www-form-urlencoded",
  Text = "text/plain",
}

export class HttpClient<SecurityDataType = unknown> {
  public baseUrl: string = "http://localhost:5265";
  private securityData: SecurityDataType | null = null;
  private securityWorker?: ApiConfig<SecurityDataType>["securityWorker"];
  private abortControllers = new Map<CancelToken, AbortController>();
  private customFetch = (...fetchParams: Parameters<typeof fetch>) =>
    fetch(...fetchParams);

  private baseApiParams: RequestParams = {
    credentials: "same-origin",
    headers: {},
    redirect: "follow",
    referrerPolicy: "no-referrer",
  };

  constructor(apiConfig: ApiConfig<SecurityDataType> = {}) {
    Object.assign(this, apiConfig);
  }

  public setSecurityData = (data: SecurityDataType | null) => {
    this.securityData = data;
  };

  protected encodeQueryParam(key: string, value: any) {
    const encodedKey = encodeURIComponent(key);
    return `${encodedKey}=${encodeURIComponent(typeof value === "number" ? value : `${value}`)}`;
  }

  protected addQueryParam(query: QueryParamsType, key: string) {
    return this.encodeQueryParam(key, query[key]);
  }

  protected addArrayQueryParam(query: QueryParamsType, key: string) {
    const value = query[key];
    return value.map((v: any) => this.encodeQueryParam(key, v)).join("&");
  }

  protected toQueryString(rawQuery?: QueryParamsType): string {
    const query = rawQuery || {};
    const keys = Object.keys(query).filter(
      (key) => "undefined" !== typeof query[key],
    );
    return keys
      .map((key) =>
        Array.isArray(query[key])
          ? this.addArrayQueryParam(query, key)
          : this.addQueryParam(query, key),
      )
      .join("&");
  }

  protected addQueryParams(rawQuery?: QueryParamsType): string {
    const queryString = this.toQueryString(rawQuery);
    return queryString ? `?${queryString}` : "";
  }

  private contentFormatters: Record<ContentType, (input: any) => any> = {
    [ContentType.Json]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string")
        ? JSON.stringify(input)
        : input,
    [ContentType.JsonApi]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string")
        ? JSON.stringify(input)
        : input,
    [ContentType.Text]: (input: any) =>
      input !== null && typeof input !== "string"
        ? JSON.stringify(input)
        : input,
    [ContentType.FormData]: (input: any) => {
      if (input instanceof FormData) {
        return input;
      }

      return Object.keys(input || {}).reduce((formData, key) => {
        const property = input[key];
        formData.append(
          key,
          property instanceof Blob
            ? property
            : typeof property === "object" && property !== null
              ? JSON.stringify(property)
              : `${property}`,
        );
        return formData;
      }, new FormData());
    },
    [ContentType.UrlEncoded]: (input: any) => this.toQueryString(input),
  };

  protected mergeRequestParams(
    params1: RequestParams,
    params2?: RequestParams,
  ): RequestParams {
    return {
      ...this.baseApiParams,
      ...params1,
      ...(params2 || {}),
      headers: {
        ...(this.baseApiParams.headers || {}),
        ...(params1.headers || {}),
        ...((params2 && params2.headers) || {}),
      },
    };
  }

  protected createAbortSignal = (
    cancelToken: CancelToken,
  ): AbortSignal | undefined => {
    if (this.abortControllers.has(cancelToken)) {
      const abortController = this.abortControllers.get(cancelToken);
      if (abortController) {
        return abortController.signal;
      }
      return void 0;
    }

    const abortController = new AbortController();
    this.abortControllers.set(cancelToken, abortController);
    return abortController.signal;
  };

  public abortRequest = (cancelToken: CancelToken) => {
    const abortController = this.abortControllers.get(cancelToken);

    if (abortController) {
      abortController.abort();
      this.abortControllers.delete(cancelToken);
    }
  };

  public request = async <T = any, E = any>({
    body,
    secure,
    path,
    type,
    query,
    format,
    baseUrl,
    cancelToken,
    ...params
  }: FullRequestParams): Promise<T> => {
    const secureParams =
      ((typeof secure === "boolean" ? secure : this.baseApiParams.secure) &&
        this.securityWorker &&
        (await this.securityWorker(this.securityData))) ||
      {};
    const requestParams = this.mergeRequestParams(params, secureParams);
    const queryString = query && this.toQueryString(query);
    const payloadFormatter = this.contentFormatters[type || ContentType.Json];
    const responseFormat = format || requestParams.format;

    return this.customFetch(
      `${baseUrl || this.baseUrl || ""}${path}${queryString ? `?${queryString}` : ""}`,
      {
        ...requestParams,
        headers: {
          ...(requestParams.headers || {}),
          ...(type && type !== ContentType.FormData
            ? { "Content-Type": type }
            : {}),
        },
        signal:
          (cancelToken
            ? this.createAbortSignal(cancelToken)
            : requestParams.signal) || null,
        body:
          typeof body === "undefined" || body === null
            ? null
            : payloadFormatter(body),
      },
    ).then(async (response) => {
      const r = response as HttpResponse<T, E>;
      r.data = null as unknown as T;
      r.error = null as unknown as E;

      const responseToParse = responseFormat ? response.clone() : response;
      const data = !responseFormat
        ? r
        : await responseToParse[responseFormat]()
            .then((data) => {
              if (r.ok) {
                r.data = data;
              } else {
                r.error = data;
              }
              return r;
            })
            .catch((e) => {
              r.error = e;
              return r;
            });

      if (cancelToken) {
        this.abortControllers.delete(cancelToken);
      }

      if (!response.ok) throw data;
      return data.data;
    });
  };
}

/**
 * @title My Title
 * @version 1.0.0
 * @baseUrl http://localhost:5265
 */
export class Api<
  SecurityDataType extends unknown,
> extends HttpClient<SecurityDataType> {
  api = {
    /**
     * No description
     *
     * @tags Approvals
     * @name ApprovalsGetPending
     * @request GET:/api/approvals/pending
     */
    approvalsGetPending: (params: RequestParams = {}) =>
      this.request<PendingRequestDto[], any>({
        path: `/api/approvals/pending`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Approvals
     * @name ApprovalsApprove
     * @request POST:/api/approvals/{id}/approve
     */
    approvalsApprove: (
      { id }: ApprovalsApproveParams,
      params: RequestParams = {},
    ) =>
      this.request<DecisionResponseDto, any>({
        path: `/api/approvals/${id}/approve`,
        method: "POST",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Approvals
     * @name ApprovalsReject
     * @request POST:/api/approvals/{id}/reject
     */
    approvalsReject: (
      { id }: ApprovalsRejectParams,
      data: RejectRequestDto,
      params: RequestParams = {},
    ) =>
      this.request<DecisionResponseDto, any>({
        path: `/api/approvals/${id}/reject`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Approvals
     * @name ApprovalsCancel
     * @request POST:/api/approvals/{id}/cancel
     */
    approvalsCancel: (
      { id }: ApprovalsCancelParams,
      params: RequestParams = {},
    ) =>
      this.request<DecisionResponseDto, any>({
        path: `/api/approvals/${id}/cancel`,
        method: "POST",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Approvals
     * @name ApprovalsRequestCancellation
     * @request POST:/api/approvals/{id}/request-cancellation
     */
    approvalsRequestCancellation: (
      { id }: ApprovalsRequestCancellationParams,
      params: RequestParams = {},
    ) =>
      this.request<DecisionResponseDto, any>({
        path: `/api/approvals/${id}/request-cancellation`,
        method: "POST",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Auth
     * @name AuthLogin
     * @request POST:/api/Auth/login
     */
    authLogin: (data: LoginRequest, params: RequestParams = {}) =>
      this.request<Blob, any>({
        path: `/api/Auth/login`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Auth
     * @name AuthLogout
     * @request POST:/api/Auth/logout
     */
    authLogout: (params: RequestParams = {}) =>
      this.request<Blob, any>({
        path: `/api/Auth/logout`,
        method: "POST",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Auth
     * @name AuthMe
     * @request GET:/api/Auth/me
     */
    authMe: (params: RequestParams = {}) =>
      this.request<CurrentLoggedUser, any>({
        path: `/api/Auth/me`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Balances
     * @name BalancesGet
     * @request GET:/api/employees/{employeeId}/balances
     */
    balancesGet: (
      { employeeId, ...query }: BalancesGetParams,
      params: RequestParams = {},
    ) =>
      this.request<BalancesResponse, any>({
        path: `/api/employees/${employeeId}/balances`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags EmployeeOverview
     * @name EmployeeOverviewGet
     * @request GET:/api/employees/overview
     */
    employeeOverviewGet: (
      query: EmployeeOverviewGetParams = {},
      params: RequestParams = {},
    ) =>
      this.request<EmployeeOverviewRow[], any>({
        path: `/api/employees/overview`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Parameters
     * @name ParametersGet
     * @request GET:/api/parameters
     */
    parametersGet: (
      query: ParametersGetParams = {},
      params: RequestParams = {},
    ) =>
      this.request<ParameterDto[], any>({
        path: `/api/parameters`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Parameters
     * @name ParametersAdd
     * @request POST:/api/parameters
     */
    parametersAdd: (data: ParameterInput, params: RequestParams = {}) =>
      this.request<ParameterVersionDto, string[]>({
        path: `/api/parameters`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Parameters
     * @name ParametersDelete
     * @request DELETE:/api/parameters/{id}
     */
    parametersDelete: (
      { id }: ParametersDeleteParams,
      params: RequestParams = {},
    ) =>
      this.request<Blob, any>({
        path: `/api/parameters/${id}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Requests
     * @name RequestsPreview
     * @request POST:/api/requests/preview
     */
    requestsPreview: (data: RequestInput, params: RequestParams = {}) =>
      this.request<RequestPreview, ProblemDetails>({
        path: `/api/requests/preview`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Requests
     * @name RequestsSubmit
     * @request POST:/api/requests
     */
    requestsSubmit: (data: RequestInput, params: RequestParams = {}) =>
      this.request<RequestDto, RequestPreview | ProblemDetails>({
        path: `/api/requests`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Transfers
     * @name TransfersGetPending
     * @request GET:/api/transfers/pending
     */
    transfersGetPending: (params: RequestParams = {}) =>
      this.request<PendingTransferDto[], any>({
        path: `/api/transfers/pending`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Transfers
     * @name TransfersApprove
     * @request POST:/api/transfers/{id}/approve
     */
    transfersApprove: (
      { id }: TransfersApproveParams,
      params: RequestParams = {},
    ) =>
      this.request<TransferDto, any>({
        path: `/api/transfers/${id}/approve`,
        method: "POST",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Transfers
     * @name TransfersReject
     * @request POST:/api/transfers/{id}/reject
     */
    transfersReject: (
      { id }: TransfersRejectParams,
      data: RejectRequestDto,
      params: RequestParams = {},
    ) =>
      this.request<TransferDto, any>({
        path: `/api/transfers/${id}/reject`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags User
     * @name UserGetAllUsers
     * @request GET:/api/User/users
     */
    userGetAllUsers: (params: RequestParams = {}) =>
      this.request<EmployeeDto[], any>({
        path: `/api/User/users`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags User
     * @name UserGetUserByEmail
     * @request GET:/api/User/users/{email}
     */
    userGetUserByEmail: (
      { email }: UserGetUserByEmailParams,
      params: RequestParams = {},
    ) =>
      this.request<EmployeeDto, any>({
        path: `/api/User/users/${email}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags User
     * @name UserCreateUser
     * @request POST:/api/User
     */
    userCreateUser: (data: CreateEmployeeRequest, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/User`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags User
     * @name UserUpdateUser
     * @request PUT:/api/User/{id}
     */
    userUpdateUser: (
      { id }: UserUpdateUserParams,
      data: UpdateEmployeeRequest,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/User/${id}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags User
     * @name UserActivateUser
     * @request POST:/api/User/activate/{id}
     */
    userActivateUser: (
      { id }: UserActivateUserParams,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/User/activate/${id}`,
        method: "POST",
        ...params,
      }),

    /**
     * No description
     *
     * @tags User
     * @name UserDeactivateUser
     * @request POST:/api/User/deactivate/{id}
     */
    userDeactivateUser: (
      { id }: UserDeactivateUserParams,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/User/deactivate/${id}`,
        method: "POST",
        ...params,
      }),

    /**
     * No description
     *
     * @tags WorkSchedules
     * @name WorkSchedulesGet
     * @request GET:/api/employees/{employeeId}/schedules
     */
    workSchedulesGet: (
      { employeeId }: WorkSchedulesGetParams,
      params: RequestParams = {},
    ) =>
      this.request<WorkSchedulesResponse, any>({
        path: `/api/employees/${employeeId}/schedules`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags WorkSchedules
     * @name WorkSchedulesAdd
     * @request POST:/api/employees/{employeeId}/schedules
     */
    workSchedulesAdd: (
      { employeeId }: WorkSchedulesAddParams,
      data: WorkScheduleInput,
      params: RequestParams = {},
    ) =>
      this.request<WorkScheduleDto, string[] | ProblemDetails>({
        path: `/api/employees/${employeeId}/schedules`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags WorkSchedules
     * @name WorkSchedulesDelete
     * @request DELETE:/api/employees/{employeeId}/schedules/{id}
     */
    workSchedulesDelete: (
      { employeeId, id }: WorkSchedulesDeleteParams,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails | string>({
        path: `/api/employees/${employeeId}/schedules/${id}`,
        method: "DELETE",
        ...params,
      }),
  };
  employees = {
    /**
     * No description
     *
     * @tags Requests
     * @name RequestsGetForEmployee
     * @request GET:/employees/{employeeId}/requests
     */
    requestsGetForEmployee: (
      { employeeId }: RequestsGetForEmployeeParams,
      params: RequestParams = {},
    ) =>
      this.request<RequestDto[], any>({
        path: `/employees/${employeeId}/requests`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Transfers
     * @name TransfersGetForEmployee
     * @request GET:/employees/{employeeId}/transfers
     */
    transfersGetForEmployee: (
      { employeeId }: TransfersGetForEmployeeParams,
      params: RequestParams = {},
    ) =>
      this.request<TransferDto[], any>({
        path: `/employees/${employeeId}/transfers`,
        method: "GET",
        format: "json",
        ...params,
      }),
  };
  teamHolidays = {
    /**
     * No description
     *
     * @tags TeamHolidays
     * @name TeamHolidaysGet
     * @request GET:/team-holidays
     */
    teamHolidaysGet: (
      query: TeamHolidaysGetParams = {},
      params: RequestParams = {},
    ) =>
      this.request<TeamCalendarResponse, ProblemDetails>({
        path: `/team-holidays`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
  transfers = {
    /**
     * No description
     *
     * @tags Transfers
     * @name TransfersCreate
     * @request POST:/transfers
     */
    transfersCreate: (data: TransferInput, params: RequestParams = {}) =>
      this.request<TransferDto, string[] | ProblemDetails>({
        path: `/transfers`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
  };
}
