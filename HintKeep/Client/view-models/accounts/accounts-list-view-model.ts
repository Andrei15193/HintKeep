import type { IObservableCollection, IReadOnlyObservableCollection } from 'react-model-view-viewmodel';
import { ViewModel, ObservableCollection } from 'react-model-view-viewmodel';

export class AccountsListViewModel extends ViewModel {
    private _accounts: readonly Account[] = [];
    private _searchText: string = '';
    private readonly _filteredAccounts: IObservableCollection<Account> = new ObservableCollection<Account>();

    public get accounts(): readonly Account[] {
        return this._accounts;
    }

    public get filteredAccounts(): IReadOnlyObservableCollection<Account> {
        return this._filteredAccounts;
    }

    public get searchText(): string {
        return this._searchText;
    }

    public set searchText(value: string) {
        if (this._searchText !== value) {
            this._searchText = value;
            this._filterAccounts();
            this.notifyPropertiesChanged('searchText');
        }
    }

    public reset(accounts: readonly Account[]) {
        if (this._accounts !== accounts) {
            this._accounts = accounts;
            this._filterAccounts();
            this.notifyPropertiesChanged('accounts');
        }
    }

    private _filterAccounts(): void {
        const searchTerms = this._searchText.toLocaleLowerCase().split(/\s+/g).filter(searchTerm => searchTerm.length > 0);
        const filteredAccounts = searchTerms.length === 0
            ? this._accounts
            : this._accounts.filter(account => {
                const accountName = account.name.toLocaleLowerCase();
                return searchTerms.some(searchTerm => accountName.includes(searchTerm));
            });
        this._filteredAccounts.reset(...filteredAccounts);
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