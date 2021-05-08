import type { AxiosInstance } from 'axios';
import type { IEvent } from '../events';
import { DispatchEvent } from '../events';
import { Axios } from '../services';
import { ViewModel } from './core';

export class AuthenticationViewModel extends ViewModel {
    private readonly _authenticated: DispatchEvent = new DispatchEvent();
    private readonly _loggedOut: DispatchEvent = new DispatchEvent();
    private readonly _axios: AxiosInstance;

    public constructor(axios: AxiosInstance = Axios) {
        super();
        this._axios = axios;
    }

    public get authenticated(): IEvent {
        return this._authenticated;
    }

    public get loggedOut(): IEvent {
        return this._loggedOut;
    }

    public authenticate(jsonWebToken: string): void {
        this._axios.defaults.headers.Authorization = `Bearer ${jsonWebToken}`;
        this._authenticated.dispatch(this);
    }

    public logOut(): void {
        this._axios.defaults.headers.Authorization = 'Bearer invalid';
        this._loggedOut.dispatch(this);
    }
}