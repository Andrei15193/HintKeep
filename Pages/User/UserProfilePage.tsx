import React from "react";
import { useAuthenticatedUser, useAuthentication } from "../../Core/Contexts/AuthenticationContext";
import { Button } from "../../Core/Forms/Components";

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
            <Button
                text="Logout"
                onClick={logOut}
            />
        </>
    );
}