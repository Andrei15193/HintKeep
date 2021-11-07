export interface IRequestData {
    readonly email: string;
    readonly hint: string;
    readonly password: string;
}

export interface IResponseData {
}

export interface IConflictResponseData {
}

export interface IUnprocessableEntityResponseData {
    readonly email: readonly string[];
    readonly hint: readonly string[];
    readonly password: readonly string[];
}