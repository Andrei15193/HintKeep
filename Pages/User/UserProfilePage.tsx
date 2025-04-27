import React from "react";
import { useUser, useUserContext } from "../Contexts/UserContext";

export function UserProfilePage(): React.JSX.Element {
    const user = useUser()!;
    const { logOut } = useUserContext();

    return (
        <>
            <p>
                {user.username}
                {" "}
                profile
            </p>
            <button onClick={logOut}>
                Logout
            </button>
        </>
    );
}