import { Store } from './store';

export class UserStore extends Store {
    private _isSessionStarted: boolean = false;

    public get isSessionActive(): boolean {
        return this._isSessionStarted;
    }

    public get isSessionInactive(): boolean {
        return !this._isSessionStarted;
    }

    public startSession(): void {
        this._isSessionStarted = true;
        this.notifyPropertyChanged('isSessionActive', 'isSessionInactive');
    }

    public completeSession(): void {
        this._isSessionStarted = false;
        this.notifyPropertyChanged('isSessionActive', 'isSessionInactive');
    }
}