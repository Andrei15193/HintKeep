import React, { useContext, useEffect } from "react";
import { useDependency } from "react-model-view-viewmodel";
import { Link, Outlet } from "react-router";
import { useAuthentication } from "../Core/Contexts/AuthenticationContext";
import { useIndexedDatabase } from "../Core/Data/IndexedDatabase";
import { Button } from "../Core/Forms/Components";
import { GlobalNotificationsContainer, Notifications } from "../Core/Notifications";
import { ConfirmationPromptContext } from "../Core/Prompt/ConfirmationPromptContext";

export function Layout(): React.JSX.Element {
    useIndexedDatabaseErrorHandler();
    const { user } = useAuthentication();

    return (
        <>
            <header>
                <div>
                    <h1>
                        HintKeep
                    </h1>
                </div>
            </header>
            <nav>
                {
                    user === null
                        ? null
                        : (
                            <Link to="/profile">
                                Profile
                            </Link>
                        )
                }
            </nav>
            <main>
                <Outlet />
            </main>
            <ConfirmationPrompt />
            <aside className="global-notifications">
                <GlobalNotificationsContainer />
            </aside>
        </>
    );
}

export function ConfirmationPrompt(): React.JSX.Element | null {
    const { confirmationPrompt } = useContext(ConfirmationPromptContext);

    if (confirmationPrompt === null)
        return null;

    const {
        options: {
            message = "Any unsaved changes will be discarded, continue?",
            confirmButtonLabel = "Yes",
            dismissButtonLabel = "No"
        } = {},
        confirm,
        dismiss
    } = confirmationPrompt;

    return (
        <aside className="confirmation-prompt">
            <div className="confirmation-prompt-content">
                <div className="confirmation-prompt-message">
                    {message}
                </div>
                <nav className="confirmation-prompt-actions">
                    <Button
                        danger
                        onClick={confirm}
                        text={confirmButtonLabel}
                    />
                    <Button
                        neutral
                        onClick={dismiss}
                        text={dismissButtonLabel}
                    />
                </nav>
            </div>
        </aside>
    );
}

export function useIndexedDatabaseErrorHandler(): void {
    const notifications = useDependency(Notifications);
    const { error, isUnavailable, initializeAsync } = useIndexedDatabase();

    useEffect(
        () => {
            if (isUnavailable && error)
                notifications.add({
                    type: "error",
                    message() {
                        return (
                            <>
                                <div>
                                    {error instanceof Error ? error.message : JSON.stringify(error)}
                                </div>
                                <Button onClick={initializeAsync}>
                                    Try again
                                </Button>
                            </>
                        );
                    }
                });
        },
        [isUnavailable, error, notifications, initializeAsync]
    );
}