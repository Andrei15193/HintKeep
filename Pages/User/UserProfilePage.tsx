import React, { useCallback } from "react";
import { Link, useNavigate } from "react-router";
import { useAuthenticatedUser, useAuthentication } from "../../Core/Contexts/AuthenticationContext";
import { Button } from "../../Core/Forms/Components";
import { Header } from "../../Core/PageParts";
import { useShowConfirmationPrompt } from "../../Core/Prompt";

export function UserProfilePage(): React.JSX.Element {
    const { username } = useAuthenticatedUser();
    const { logOut } = useAuthentication();
    const navigate = useNavigate();

    const showGreatPrompt = useShowConfirmationPrompt({
        message: "Awesome, thank you! What do you want to do next?",
        confirmButtonLabel: "Take me to the accounts page",
        dismissButtonLabel: "Log out :)",
        onConfirm: useCallback(
            () => {
                navigate("/");
            },
            [navigate]
        ),
        onDismiss: logOut
    });

    const showLogoutPrompt = useShowConfirmationPrompt({
        message: "We will really miss you! Are you sure you don't want to stay?",
        confirmButtonLabel: "Seriously, log me out",
        dismissButtonLabel: "I wouldn't mind staying for a bit longer",
        onConfirm: logOut,
        onDismiss: useCallback(
            () => setTimeout(() => showGreatPrompt(), 0),
            [showGreatPrompt]
        )
    });

    return (
        <>
            <Header>
                {`HintKeep - ${username} Profile`}
            </Header>

            <nav>
                <Link to="/">
                    Accounts
                </Link>
                <Link to="/archived">
                    Archived Accounts
                </Link>
                <Link to="/profile">
                    Profile
                </Link>
            </nav>

            <hr />

            <p>
                We are sorry to see you go so soon, hope you had a good time!
            </p>
            <Button
                text="Logout"
                danger
                onClick={showLogoutPrompt}
            />
        </>
    );
}