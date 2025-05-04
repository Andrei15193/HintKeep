import React from "react";
import { useAuthenticatedUser, useAuthentication } from "../../Core/Contexts/AuthenticationContext";

export function UserProfilePage(): React.JSX.Element {
    const user = useAuthenticatedUser();
    const { logOut } = useAuthentication();

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