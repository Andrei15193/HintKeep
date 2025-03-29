export interface IAccount {
    readonly id: string;
    readonly name: string;
    readonly username: string;
    readonly hint: string;
    readonly isPinned: boolean;
    readonly notes: string;
}