interface IAccountHintResponse {
    readonly id: string;
    readonly hint: string;
    readonly dateAdded: string;
}

export type IResponseData = readonly IAccountHintResponse[];