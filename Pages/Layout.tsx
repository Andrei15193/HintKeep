import React, { useEffect } from "react";
import { useDependency } from "react-model-view-viewmodel";
import { Outlet } from "react-router";
import { useIndexedDatabase } from "../Data/IndexedDatabase";
import { GlobalNotificationsContainer, Notifications } from "./Notifications";

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
            <aside className="global-notifications">
                <GlobalNotificationsContainer />
            </aside>
        </>
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