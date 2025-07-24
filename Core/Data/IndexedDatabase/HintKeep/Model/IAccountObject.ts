export type IAccountObject = IAccountSummaryObject | IAccountHintObject;
export type AccountObjectType = IAccountObject["type"];

export interface IAccountSummaryObject {
    readonly type: "summary";

    readonly userId: string;
    readonly id: string;
    readonly status: "active" | "archived";
    readonly name: string;
    readonly username: string;
    readonly hint: string;
    readonly isPinned: boolean;
    readonly notes: string;
}

export interface IAccountHintObject {
    readonly type: "hint";

    readonly userId: string;
    readonly accountId: string;
    readonly id: string;
    readonly hint: string;
    readonly dateAdded: string;
}