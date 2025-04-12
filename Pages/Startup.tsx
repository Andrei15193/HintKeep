import React from "react";
import { IndexedDatabaseProvider } from "../Data/IndexedDatabase";
import { HintKeepDatabaseDefinition } from "../Data/IndexedDatabase/HintKeep";
import { HintKeepDependencyContainerProvider } from "../Dependencies";
import { UserContextProvider } from "./Contexts/UserContext";
import { AppRouter } from "./Navigation/AppRouter";

export function Startup(): React.JSX.Element {
    return (
        <UserContextProvider>
            <IndexedDatabaseProvider databaseDefinition={HintKeepDatabaseDefinition}>
                <HintKeepDependencyContainerProvider>
                    <AppRouter />
                </HintKeepDependencyContainerProvider>
            </IndexedDatabaseProvider>
        </UserContextProvider>
    );
}