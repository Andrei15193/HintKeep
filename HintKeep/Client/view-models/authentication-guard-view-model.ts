import type { AxiosResponse } from 'axios';
import type { IResponseData } from '../api/users/details/get';
import { Axios } from '../services';
import { ApiViewModel } from './core';

export class AuthenticationGuardViewModel extends ApiViewModel {
    private _isAuthenticationEnsured: boolean = false;

    public constructor() {
        super(Axios);
    }

    public get isAuthenticationEnsured(): boolean {
        return this._isAuthenticationEnsured;
    }

    public ensureAuthenticatedAsync(): Promise<void> {
        if (!this._isAuthenticationEnsured)
            return this
                .get('/api/users/details')
                .on(204, (_: AxiosResponse<IResponseData>) => {
                    this._isAuthenticationEnsured = true;
                    this.notifyPropertyChanged('isAuthenticationEnsured');
                })
                .sendAsync();
        else
            return Promise.resolve();
    }
}