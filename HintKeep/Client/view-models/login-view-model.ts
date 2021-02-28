import { userStore } from '../stores';
import { UserInfo } from '../stores/user-info';
import { AsyncViewModel } from './core';

export class LoginViewModel extends AsyncViewModel {
    private _email: string = "";
    private _password: string = "";

    public constructor() {
        super(userStore);
        const { userInfo } = userStore;
        if (userInfo !== null)
            this._email = userInfo.email;
    }

    public get email() {
        return this._email;
    }

    public set email(value: string) {
        this._email = value || '';
        this.notifyChanged();
    }

    public get password() {
        return this._password;
    }

    public set password(value: string) {
        this._password = value || '';
        this.notifyChanged();
    }

    public async loginAsync(): Promise<void> {
        await this.handleAsync(async () => {
            await this.delay(3000);
            userStore.startSession(new UserInfo(this.email));
        });
    }

    public logOut(): void {
        userStore.completeSession();
    }
}