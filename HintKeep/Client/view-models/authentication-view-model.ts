import type { AxiosInstance } from 'axios';
import type { IEvent } from 'react-model-view-viewmodel';
import { ViewModel, DispatchEvent } from 'react-model-view-viewmodel';
import { Axios } from '../services';

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
        Object.assign(this._axios.defaults.headers, { Authorization: `Bearer ${jsonWebToken}` });
        this._authenticated.dispatch(this);
    }

    public logOut(): void {
        Object.assign(this._axios.defaults.headers, { Authorization: 'Bearer invalid' });
        this._loggedOut.dispatch(this);
    }
}