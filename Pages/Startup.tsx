import React from "react";
import { RouterProvider } from "react-router";
import { AuthenticationContextProvider } from "../Core/Contexts/AuthenticationContext";
import { IndexedDatabaseProvider } from "../Core/Data/IndexedDatabase";
import { HintKeepDatabaseDefinition } from "../Core/Data/IndexedDatabase/HintKeep";
import { HintKeepDependencyContainerProvider } from "../Core/Dependencies";
import { GlobalNotificationsContainer } from "../Core/Notifications";
import { ConfirmationPromptProvider } from "../Core/Prompt";
import { AppRouter } from "./AppRouter";
import { ConfirmationPrompt } from "./Layout";

export function Startup(): React.JSX.Element {
    return (
        <AuthenticationContextProvider>
            <ConfirmationPromptProvider>
                <IndexedDatabaseProvider databaseDefinition={HintKeepDatabaseDefinition}>
                    <HintKeepDependencyContainerProvider>
                        <RouterProvider router={AppRouter} />

                        <ConfirmationPrompt />
                        <aside className="global-notifications">
                            <GlobalNotificationsContainer />
                        </aside>
                    </HintKeepDependencyContainerProvider>
                </IndexedDatabaseProvider>
            </ConfirmationPromptProvider>
        </AuthenticationContextProvider>
    );
}