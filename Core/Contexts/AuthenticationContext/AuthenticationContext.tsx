import type { IUser } from "../../Models";
import React, { type PropsWithChildren, createContext, useCallback, useContext, useMemo, useState } from "react";

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
    const [user, setUser] = useState<IUser | null>(null);

    const logOutCallback = useCallback(
        () => {
            setUser(null);
        },
        [setUser]
    );

    const authenticationContext = useMemo<IAuthenticationContext>(
        () => ({
            user,
            authenticate: setUser,
            logOut: logOutCallback
        }),
        [user, logOutCallback, setUser]
    );

    return (
        <AuthenticationContext.Provider
            value={authenticationContext}
            children={children}
        />
    );
}