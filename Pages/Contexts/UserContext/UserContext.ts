import type { IUser } from "../../Model/IUser";
import { createContext } from "react";

export interface IUserContext {
    readonly user: IUser | null;

    authenticate(user: IUser): void;
    logOut(): void;
}

export const UserContext = createContext<IUserContext | null>(null);