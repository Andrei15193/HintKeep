import React from "react";
import { IndexedDatabaseProvider } from "../Data/IndexedDatabase";
import { HintKeepDatabaseDefinition } from "../Data/IndexedDatabase/HintKeep";
import { HintKeepDependencyContainerProvider } from "../Dependencies";
import { UserContextProvider } from "./Contexts/UserContext";
import { AppRouter } from "./Navigation/AppRouter";
import { ConfirmationPromptProvider } from "./Prompt";

export function Startup(): React.JSX.Element {
    return (
        <ConfirmationPromptProvider>
            <UserContextProvider>
                <IndexedDatabaseProvider databaseDefinition={HintKeepDatabaseDefinition}>
                    <HintKeepDependencyContainerProvider>
                        <AppRouter />
                    </HintKeepDependencyContainerProvider>
                </IndexedDatabaseProvider>
            </UserContextProvider>
        </ConfirmationPromptProvider>
    );
}