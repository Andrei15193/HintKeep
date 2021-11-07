import type { AxiosResponse } from 'axios';
import type { IResponseData as IGetResponseData } from '../../api/accounts-hints/get';
import type { INotFoundResponseData as INotFoundDeleteResponseData, IResponseData as IDeleteResponseData } from '../../api/accounts-hints/delete';
import { ApiViewModel } from '../api-view-model';

export class AccountHintsViewModel extends ApiViewModel {
    private _accountId: string | null = null;
    private _accountHints: readonly AccountHint[] = [];

    public get accountHints(): readonly AccountHint[] {
        return this._accountHints;
    }

    public loadAsync(accountId: string): Promise<void> {
        return this
            .get(`/api/accounts/${accountId}/hints`)
            .on(200, (response: AxiosResponse<IGetResponseData>) => {
                this._accountId = accountId;
                this._accountHints = response.data.map(account => new AccountHint(account.id, account.hint, new Date(account.dateAdded)));
                this.notifyPropertiesChanged('accountHints');
            })
            .sendAsync();
    }

    public async deleteAsync(hintId: string): Promise<void> {
        if (this._accountId !== null)
            await this
                .delete(`/api/accounts/${this._accountId}/hints/${hintId}`)
                .on(204, (_: AxiosResponse<IDeleteResponseData>) => {
                    this._accountHints = this._accountHints.filter(accountHint => accountHint.id !== hintId);
                    this.notifyPropertiesChanged('accountHints');
                })
                .on(404, (_: AxiosResponse<INotFoundDeleteResponseData>) => {
                })
                .sendAsync();
    }
}

export class AccountHint {
    public constructor(id: string, hint: string, dateAdded: Date) {
        this.id = id;
        this.hint = hint;
        this.dateAdded = dateAdded;
    }

    public readonly id: string;

    public readonly hint: string;

    public readonly dateAdded: Date;
}