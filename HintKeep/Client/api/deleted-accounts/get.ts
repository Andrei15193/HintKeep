interface IAccountResponse {
    readonly id: string;
    readonly name: string;
    readonly hint: string;
    readonly isPinned: boolean;
}

export type IResponseData = readonly IAccountResponse[];