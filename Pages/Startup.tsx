import React from "react";
import { RouterProvider } from "react-router";
import { AuthenticationContextProvider } from "../Core/Contexts/AuthenticationContext";
import { IndexedDatabaseProvider } from "../Core/Data/IndexedDatabase";
import { HintKeepDatabaseDefinition } from "../Core/Data/IndexedDatabase/HintKeep";
import { HintKeepDependencyContainerProvider } from "../Core/Dependencies";
import { AppRouter } from "./AppRouter";
import { ConfirmationPromptProvider } from "./Prompt";

export function Startup(): React.JSX.Element {
    return (
        <AuthenticationContextProvider>
            <ConfirmationPromptProvider>
                <IndexedDatabaseProvider databaseDefinition={HintKeepDatabaseDefinition}>
                    <HintKeepDependencyContainerProvider>
                        <RouterProvider router={AppRouter} />
                    </HintKeepDependencyContainerProvider>
                </IndexedDatabaseProvider>
            </ConfirmationPromptProvider>
        </AuthenticationContextProvider>
    );
}