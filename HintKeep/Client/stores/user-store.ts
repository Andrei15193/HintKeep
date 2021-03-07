import { Store } from './store';
import { UserInfo } from './user-info';

export class UserStore extends Store {
    private _userInfo: UserInfo | null = null;

    public get isSessionActive(): boolean {
        return this._userInfo !== null;
    }

    public get isSessionInactive(): boolean {
        return this._userInfo === null;
    }

    public get userInfo(): UserInfo | null {
        return this._userInfo;
    }

    public startSession(userInfo: UserInfo): void {
        this._userInfo = userInfo;
        this.notifyPropertyChanged('isSessionActive', 'isSessionInactive', 'userInfo');
    }

    public completeSession(): void {
        this._userInfo = null;
        this.notifyPropertyChanged('isSessionActive', 'isSessionInactive', 'userInfo');
    }
}