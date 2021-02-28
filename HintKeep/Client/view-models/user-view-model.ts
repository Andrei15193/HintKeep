import { userStore } from '../stores';
import { ViewModel } from './core';

export class UserViewModel extends ViewModel {
    public constructor() {
        super(userStore);
    }

    public get isAuthenticated(): boolean {
        return userStore.isSessionActive;
    }
}