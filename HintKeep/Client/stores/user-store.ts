import { Axios } from '../services';
import { Store } from './store';

export class UserStore extends Store {
    private _isSessionStarted: boolean = false;

    public constructor() {
        super();
        const jsonWebToken = localStorage.getItem('jsonWebToken');
        if (jsonWebToken)
            this.startSession(jsonWebToken);
        else
            this.completeSession();
    }

    public get isSessionActive(): boolean {
        return this._isSessionStarted;
    }

    public get isSessionInactive(): boolean {
        return !this._isSessionStarted;
    }

    public startSession(jsonWebToken: string): void {
        Axios.defaults.headers.Authorization = `Bearer ${jsonWebToken}`;
        localStorage.setItem('jsonWebToken', jsonWebToken);
        this._isSessionStarted = true;

        this.notifyPropertyChanged('isSessionActive', 'isSessionInactive');
    }

    public completeSession(): void {
        localStorage.removeItem('jsonWebToken');
        Axios.defaults.headers.Authorization = null;
        this._isSessionStarted = false;

        this.notifyPropertyChanged('isSessionActive', 'isSessionInactive');
    }
}