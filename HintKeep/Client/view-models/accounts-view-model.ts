import type { AxiosResponse } from 'axios';
import type { IResponseData } from '../api/accounts/get';
import { Axios } from '../services';
import { ApiViewModel } from './core';

export class AccountsViewModel extends ApiViewModel {
    private _accounts: readonly Account[];
    private _searchText: string = '';

    public constructor() {
        super(Axios);
        this._accounts = [];
    }

    public get hasAccounts(): boolean {
        return this._accounts.length > 0;
    }

    public get accounts(): readonly Account[] {
        const searchTerms = this._searchText.toLocaleLowerCase().split(/\s+/g).filter(searchTerm => searchTerm.length > 0);
        if (searchTerms.length === 0)
            return this._accounts;
        else
            return this._accounts.filter(account => {
                const accountName = account.name.toLocaleLowerCase();
                return searchTerms.some(searchTerm => accountName.includes(searchTerm));
            });
    }

    public get searchText(): string {
        return this._searchText;
    }

    public set searchText(value: string) {
        if (this._searchText !== value) {
            this._searchText = value || '';
            this.notifyPropertyChanged('searchText', 'accounts');
        }
    }

    public loadAsync(): Promise<void> {
        return this
            .get("/api/accounts")
            .on(200, (response: AxiosResponse<IResponseData>) => {
                this._accounts = response.data.map(account => new Account(account.id, account.name, account.hint, account.isPinned));
                this.notifyPropertyChanged('accounts', 'hasAccounts');
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