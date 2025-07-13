export interface IAccountObject {
    readonly userId: string;
    readonly id: string;
    readonly status: "active" | "archived";
    readonly name: string;
    readonly username: string;
    readonly hint: string;
    readonly isPinned: boolean;
    readonly notes: string;
}