export interface IRequestData {
    readonly email: string;
    readonly token: string;
    readonly password: string;
}

export interface IResponseData {
}

export interface INotFoundResponseData {
}

export interface IUnprocessableEntityResponseData {
    readonly email: readonly string[];
    readonly token: readonly string[];
    readonly password: readonly string[];
}