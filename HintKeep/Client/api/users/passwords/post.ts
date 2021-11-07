export interface IRequestData {
    readonly token: string;
    readonly password: string;
}

export interface IResponseData {
}

export interface INotFoundResponseData {
}

export interface IUnprocessableEntityResponseData {
    readonly token: readonly string[];
    readonly password: readonly string[];
}