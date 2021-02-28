export class UserInfo {
    private readonly _email: string;

    public constructor(email: string) {
        this._email = email;
    }

    public get email(): string {
        return this._email;
    }
}