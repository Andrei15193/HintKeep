import type { AxiosResponse } from 'axios';
import type { IResponseData } from '../../api/accounts/get';
import { AccountsListViewModel, Account } from './accounts-list-view-model';
import { ApiViewModel } from '../api-view-model';

export class AccountsViewModel extends ApiViewModel {
    private readonly _accounts: AccountsListViewModel = new AccountsListViewModel();

    public get accounts(): AccountsListViewModel {
        return this._accounts;
    }

    public loadAsync(): Promise<void> {
        return this
            .get("/api/accounts")
            .on(200, (response: AxiosResponse<IResponseData>) => {
                this._accounts.reset(response.data.map(account => new Account(account.id, account.name, account.hint, account.isPinned)));
                this.notifyPropertiesChanged('accounts', 'hasAccounts');
            })
            .sendAsync();
    }
}