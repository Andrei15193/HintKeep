export interface IResponseData {
    readonly id: string;
    readonly name: string;
    readonly hint: string;
    readonly isPinned: boolean;
    readonly notes: string;
}

export interface INotFoundResponseData {
}