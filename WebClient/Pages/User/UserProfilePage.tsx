import React, { useCallback } from "react";
import { useDependency } from "react-model-view-viewmodel";
import { type NonIndexRouteObject, useNavigate } from "react-router";
import { UserHandler } from "../../Core/Authentication";
import { Button } from "../../Core/Forms/Components";
import { Breadcrumbs, GlobalNavigation, Header } from "../../Core/PageParts";
import { useShowConfirmationPrompt } from "../../Core/Prompt";

export const UserProfileRoute: NonIndexRouteObject = {
    path: "profile",
    Component: UserProfilePage
};

function UserProfilePage(): React.JSX.Element {
    const userHandler = useDependency(UserHandler);
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
        onDismiss() {
            userHandler.logOut();
        }
    });

    const showLogoutPrompt = useShowConfirmationPrompt({
        message: "We will really miss you! Are you sure you don't want to stay?",
        confirmButtonLabel: "Seriously, log me out",
        dismissButtonLabel: "I wouldn't mind staying for a bit longer",
        onConfirm() {
            userHandler.logOut();
        },
        onDismiss: useCallback(
            () => setTimeout(() => showGreatPrompt(), 0),
            [showGreatPrompt]
        )
    });

    return (
        <>
            <Header>
                HintKeep - Profile
            </Header>

            <GlobalNavigation />

            <Breadcrumbs items={["Profile"]} />

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