export interface IRequestData {
    readonly email: string;
    readonly confirmationToken: string
}

export interface IResponseData {
}

export interface IPreconditionFailedResponseData {
}

export interface IUnprocessableEntityResponseData {
    readonly email: readonly string[];
    readonly confirmationToken: readonly string[];
}