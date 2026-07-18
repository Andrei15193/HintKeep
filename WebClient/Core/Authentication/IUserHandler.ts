import type { IUser } from "../Models";
import type { ICurrentUserProvider } from "./ICurrentUserProvider";

export interface IUserHandler extends ICurrentUserProvider {
    authenticate(user: IUser): void;
    logOut(): void;
}