import React from "react";
import { RouterProvider } from "react-router";
import { AuthenticationContextProvider } from "../Core/Contexts/AuthenticationContext";
import { IndexedDatabaseProvider } from "../Core/Data/IndexedDatabase";
import { HintKeepDatabaseDefinition } from "../Core/Data/IndexedDatabase/HintKeep";
import { HintKeepDependencyContainerProvider } from "../Core/Dependencies";
import { GlobalNotificationsContainer } from "../Core/Notifications";
import { ConfirmationPromptProvider } from "../Core/Prompt";
import { useAppRouter } from "./AppRouter";
import { ConfirmationPrompt } from "./ConfirmationPrompt";

export function Startup(): React.JSX.Element {
    const appRouter = useAppRouter();

    return (
        <AuthenticationContextProvider>
            <ConfirmationPromptProvider>
                <IndexedDatabaseProvider databaseDefinition={HintKeepDatabaseDefinition}>
                    <HintKeepDependencyContainerProvider>
                        <RouterProvider router={appRouter} />

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