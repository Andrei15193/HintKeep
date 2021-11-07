export interface IRequestData {
    readonly token: string;
}

export interface IResponseData {
}

export interface INotFoundResponseData {
}

export interface IUnprocessableEntityResponseData {
    readonly token: readonly string[];
}