import React, { type PropsWithChildren } from "react";
import { DependencyContainer, DependencyResolverProvider, type IDependencyResolver, useDependency } from "react-model-view-viewmodel";
import { Outlet } from "react-router";
import { IndexedDatabase, IndexedDatabaseProvider, useIndexedDatabase } from "../Data/IndexedDatabase";
import { HintKeepDatabaseDefinition } from "../Data/IndexedDatabase/HintKeep";

export function Layout(): React.JSX.Element {
    return (
        <IndexedDatabaseProvider databaseDefinition={HintKeepDatabaseDefinition}>
            <HintKeepDependencyContainerProvider>
                <h1>
                    HintKeep
                </h1>
                <IndexedDatabaseErrorHandler />
                <div>
                    <Outlet />
                </div>
            </HintKeepDependencyContainerProvider>
        </IndexedDatabaseProvider>
    );
}

export function HintKeepDependencyContainerProvider({ children }: PropsWithChildren<{}>): React.JSX.Element {
    const { database } = useIndexedDatabase();

    const hintKeepDependencyContainer = useDependency(HintKeepDependencyContainer, [database]);

    return (
        <DependencyResolverProvider
            dependencyResolver={hintKeepDependencyContainer}
            children={children}
        />
    );
}

export class HintKeepDependencyContainer extends DependencyContainer {
    public constructor(_: IDependencyResolver, database: IDBDatabase | null) {
        super();

        if (database === null) {
        }
        else {
            this.registerInstanceToToken(IndexedDatabase, database);
        }
    }
}

export function IndexedDatabaseErrorHandler(): React.JSX.Element | null {
    const { error, isUnavailable, initializeAsync } = useIndexedDatabase();

    if (isUnavailable && error)
        return (
            <>
                <div>
                    Oops... something went wrong, wanna
                    {" "}
                    <button onClick={initializeAsync}>
                        try again
                    </button>
                    ?
                </div>

                <div>
                    {error instanceof Error ? error.message : JSON.stringify(error)}
                </div>
            </>
        );

    return null;
}