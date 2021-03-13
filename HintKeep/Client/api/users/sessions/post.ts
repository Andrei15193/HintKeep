export interface IRequestData {
    readonly email: string;
    readonly password: string;
}

export interface IResponseData {
    readonly jsonWebToken: string;
}

export interface IUnauthorizedResponseData {
}

export interface IUnprocessableEntityResponseData {
    readonly email: readonly string[];
    readonly password: readonly string[];
}