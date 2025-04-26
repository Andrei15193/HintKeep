import React, { useEffect } from "react";
import { useDependency } from "react-model-view-viewmodel";
import { Outlet } from "react-router";
import { useIndexedDatabase } from "../Data/IndexedDatabase";
import { GlobalNotificationsContainer, Notifications } from "./Notifications";
import { useConfirmationPrompt } from "./Prompt/ConfirmationPromptContext";

export function Layout(): React.JSX.Element {
    useIndexedDatabaseErrorHandler();

    return (
        <>
            <header>
                <h1>
                    HintKeep
                </h1>
            </header>
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
    const { confirmationPrompt } = useConfirmationPrompt();

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
                    <button
                        type="button"
                        onClick={confirm}
                    >
                        {confirmButtonLabel}
                    </button>
                    <button
                        type="button"
                        onClick={dismiss}
                    >
                        {dismissButtonLabel}
                    </button>
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
                                <button onClick={initializeAsync}>
                                    Try again
                                </button>
                            </>
                        );
                    }
                });
        },
        [isUnavailable, error, notifications, initializeAsync]
    );
}