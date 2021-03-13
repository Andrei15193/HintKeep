import { userStore } from '../../stores';
import { ViewModel } from '../core';

export class UserViewModel extends ViewModel {
    public constructor() {
        super(userStore);
    }

    public get isAuthenticated(): boolean {
        return userStore.isSessionActive;
    }

    protected storeChanged(store: object, changedProperties: readonly string[]): void {
        if (store === userStore && changedProperties.includes("isSessionActive"))
            this.notifyPropertyChanged("isAuthenticated");
    }
}