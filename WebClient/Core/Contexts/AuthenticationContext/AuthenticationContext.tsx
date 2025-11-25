import type { IUser } from "../../Models";
import React, { type PropsWithChildren, createContext, useCallback, useContext, useMemo, useState } from "react";
import { useWindow } from "../../../Pages/WindowContext";

/** It is expected to have this configured in app startup. */
const AuthenticationContext = createContext<IAuthenticationContext | null>(null);

export interface IAuthenticationContext {
    readonly user: IUser | null;

    authenticate(user: IUser): void;
    logOut(): void;
}

export function useAuthentication(): IAuthenticationContext {
    return useContext(AuthenticationContext)!;
}

export interface IUserContextProviderProps {
}

export function AuthenticationContextProvider({ children }: PropsWithChildren<IUserContextProviderProps>): React.JSX.Element {
    const { sessionStorage } = useWindow();
    const [user, setUser] = useState<IUser | null>(JSON.parse(sessionStorage.getItem("user") ?? "null"));

    const authenticateCallback = useCallback(
        (user: IUser) => {
            setUser(user);
            sessionStorage.setItem("user", JSON.stringify(user));
        },
        [sessionStorage, setUser]
    );

    const logOutCallback = useCallback(
        () => {
            setUser(null);
            sessionStorage.removeItem("user");
        },
        [sessionStorage, setUser]
    );

    const authenticationContext = useMemo<IAuthenticationContext>(
        () => ({
            user,
            authenticate: authenticateCallback,
            logOut: logOutCallback
        }),
        [user, authenticateCallback, logOutCallback]
    );

    return (
        <AuthenticationContext.Provider
            value={authenticationContext}
            children={children}
        />
    );
}