export interface IRequestData {
    readonly name: string;
    readonly hint: string;
    readonly isPinned: boolean;
    readonly notes: string;
}

export interface IResponseData {
}

export interface INotFoundResponseData {
}

export interface IConflictResponseData {
}

export interface IUnprocessableEntityResponseData {
    readonly name: readonly string[];
    readonly hint: readonly string[];
}