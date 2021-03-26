import type { AxiosResponse } from 'axios';
import type { IResponseData } from '../api/accounts/get';
import { Axios } from '../services';
import { ApiViewModel } from './core';

export class AccountsViewModel extends ApiViewModel {
    private _accounts: readonly Account[];

    public constructor() {
        super(Axios);
        this._accounts = [];
    }

    public get accounts(): readonly Account[] {
        return this._accounts;
    }

    public loadAsync(): Promise<void> {
        return this
            .get("/api/accounts")
            .on(200, (response: AxiosResponse<IResponseData>) => {
                this._accounts = response.data.map(account => new Account(account.id, account.name, account.hint, account.isPinned));
            })
            .sendAsync();
    }
}

export class Account {
    public constructor(id: string, name: string, hint: string, isPinned: boolean) {
        this.id = id;
        this.name = name;
        this.hint = hint;
        this.isPinned = isPinned;
    }

    public readonly id: string;

    public readonly name: string;

    public readonly hint: string;

    public readonly isPinned: boolean;
}