import type { IUser } from "../../Model/IUser";
import React, { useMemo, useState, type PropsWithChildren } from "react";
import { type IUserContext, UserContext } from "./UserContext";

export interface IUserContextProviderProps {
}

export function UserContextProvider({ children }: PropsWithChildren<IUserContextProviderProps>): React.JSX.Element {
    const [user, setUser] = useState<IUser | null>(null);

    const userContext = useMemo<IUserContext>(
        () => ({
            user,
            authenticate(user) {
                setUser(user);
            },
            logOut() {
                setUser(null);
            }
        }),
        [user]
    );

    return (
        <UserContext
            value={userContext}
            children={children}
        />
    );
}