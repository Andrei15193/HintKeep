export interface IRequestData {
    readonly email: string;
    readonly password: string;
}

export type IResponseData = string;

export interface IUnprocessableEntityResponseData {
    readonly '*': readonly string[];
    readonly email: readonly string[];
    readonly password: readonly string[];
}