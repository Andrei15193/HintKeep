export interface IAccountDetails {
    readonly id: string;
    readonly name: string;
    readonly username: string;
    readonly hint: string;
    readonly isPinned: boolean;
    readonly notes: string;
    readonly isArchived: boolean;
}