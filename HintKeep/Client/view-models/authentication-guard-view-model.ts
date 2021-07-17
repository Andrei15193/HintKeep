import type { AxiosResponse } from 'axios';
import type { IResponseData } from '../api/users/details/get';
import { ApiViewModel } from './api-view-model';

export class AuthenticationGuardViewModel extends ApiViewModel {
    private _isAuthenticationEnsured: boolean = false;

    public get isAuthenticationEnsured(): boolean {
        return this._isAuthenticationEnsured;
    }

    public ensureAuthenticatedAsync(): Promise<void> {
        if (!this._isAuthenticationEnsured)
            return this
                .get('/api/users/details')
                .on(204, (_: AxiosResponse<IResponseData>) => {
                    this._isAuthenticationEnsured = true;
                    this.notifyPropertiesChanged('isAuthenticationEnsured');
                })
                .sendAsync();
        else
            return Promise.resolve();
    }

    public reset(): void {
        this._isAuthenticationEnsured = false;
        this.notifyPropertiesChanged('isAuthenticationEnsured');
    }
}