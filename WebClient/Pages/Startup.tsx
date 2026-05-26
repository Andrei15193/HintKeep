import React, { type PropsWithChildren } from "react";
import { AuthenticationContextProvider } from "../Core/Contexts/AuthenticationContext";
import { IndexedDatabaseProvider } from "../Core/Data/IndexedDatabase";
import { HintKeepDatabaseDefinition } from "../Core/Data/IndexedDatabase/HintKeep";
import { HintKeepDependencyContainerProvider } from "../Core/Dependencies";
import { ConfirmationPromptProvider } from "../Core/Prompt";
import { App } from "./App";

export function Startup({ children = <App /> }: PropsWithChildren<{}>): React.JSX.Element {
    return (
        <AuthenticationContextProvider>
            <ConfirmationPromptProvider>
                <IndexedDatabaseProvider databaseDefinition={HintKeepDatabaseDefinition}>
                    <HintKeepDependencyContainerProvider>
                        {children}
                    </HintKeepDependencyContainerProvider>
                </IndexedDatabaseProvider>
            </ConfirmationPromptProvider>
        </AuthenticationContextProvider>
    );
}