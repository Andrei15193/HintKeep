export interface IRequestData {
    readonly email: string;
}

export interface IResponseData {
}

export interface IUnprocessableEntityResponseData {
    readonly email: readonly string[];
}