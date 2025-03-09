import React from "react";
import { Outlet } from "react-router";
import { IndexedDatabaseProvider, useIndexedDatabase } from "../Data/IndexedDatabase";
import { HintKeepDatabaseDefinition } from "../Data/IndexedDatabase/HintKeep";

export function Layout(): React.JSX.Element {
    return (
        <IndexedDatabaseProvider databaseDefinition={HintKeepDatabaseDefinition}>
            <h1>
                HintKeep
            </h1>
            <IndexedDatabaseErrorHandler />
            <div>
                <Outlet />
            </div>
        </IndexedDatabaseProvider>
    );
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