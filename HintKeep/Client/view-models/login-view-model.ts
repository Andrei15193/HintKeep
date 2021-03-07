import { Axios } from '../services';
import { userStore } from '../stores';
import { UserInfo } from '../stores/user-info';
import { ApiViewModel } from './core';

export class LoginViewModel extends ApiViewModel {
    private _email: string = "";
    private _password: string = "";

    public constructor() {
        super(Axios, userStore);
        const { userInfo } = userStore;
        if (userInfo !== null)
            this._email = userInfo.email;
    }

    public get email() {
        return this._email;
    }

    public set email(value: string) {
        this._email = value || '';
        this.notifyPropertyChanged('email');
    }

    public get password() {
        return this._password;
    }

    public set password(value: string) {
        this._password = value || '';
        this.notifyPropertyChanged('password');
    }

    public async loginAsync(): Promise<void> {
        await this.delay(3000);
        userStore.startSession(new UserInfo(this.email));
    }

    public logOut(): void {
        userStore.completeSession();
    }
}