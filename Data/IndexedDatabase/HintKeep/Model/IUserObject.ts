export interface IUserObject {
    readonly id: string;
    readonly username: string;
    readonly passwordHash: string;
    readonly hint: string;
}