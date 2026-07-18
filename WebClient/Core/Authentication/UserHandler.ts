import type { IUser } from "../Models";
import type { IUserHandler } from "./IUserHandler";
import { ViewModel } from "react-model-view-viewmodel";

export class UserHandler extends ViewModel implements IUserHandler {
    private _user: IUser | null = null;

    public get user(): IUser | null {
        return this._user;
    }

    public authenticate(user: IUser): void {
        if (this._user !== user) {
            this._user = user;
            this.notifyPropertiesChanged("user");
        }
    }

    public logOut(): void {
        if (this._user !== null) {
            this._user = null;
            this.notifyPropertiesChanged("user");
        }
    }
}