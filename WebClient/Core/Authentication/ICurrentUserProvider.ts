import type { IUser } from "../Models";
import { DependencyToken, type INotifyPropertiesChanged } from "react-model-view-viewmodel";

export interface ICurrentUserProvider extends INotifyPropertiesChanged {
    readonly user: IUser | null;
}

export const CurrentUser = new DependencyToken<IUser>("current user");
export const CurrentUserProvider = new DependencyToken<ICurrentUserProvider>("current user provider");