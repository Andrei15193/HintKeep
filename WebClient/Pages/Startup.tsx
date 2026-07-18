import React, { type PropsWithChildren } from "react";
import { IndexedDatabaseProvider } from "../Core/Data/IndexedDatabase";
import { HintKeepDatabaseDefinition } from "../Core/Data/IndexedDatabase/HintKeep";
import { HintKeepDependencyContainerProvider } from "../Core/Dependencies";
import { ModalContextProvider } from "../Core/Modals";
import { ConfirmationPromptProvider } from "../Core/Prompt";
import { App } from "./App";

export function Startup({ children = <App /> }: PropsWithChildren<{}>): React.JSX.Element {
    return (
        <ModalContextProvider>
            <ConfirmationPromptProvider>
                <IndexedDatabaseProvider databaseDefinition={HintKeepDatabaseDefinition}>
                    <HintKeepDependencyContainerProvider>
                        {children}
                    </HintKeepDependencyContainerProvider>
                </IndexedDatabaseProvider>
            </ConfirmationPromptProvider>
        </ModalContextProvider>
    );
}