import React, { type PropsWithChildren, useEffect } from "react";
import { Outlet } from "react-router";
import { useIndexedDatabase } from "../../Core/Data/IndexedDatabase";

export interface IIndexedDatabaseScopedProps {
}

export function IndexedDatabaseScoped({ children = <Outlet /> }: PropsWithChildren<IIndexedDatabaseScopedProps>): React.JSX.Element {
    const { isReady, isUninitialized, initializeAsync } = useIndexedDatabase();

    useEffect(
        () => {
            if (isUninitialized)
                initializeAsync();
        },
        [isUninitialized, initializeAsync]
    );

    if (isReady)
        return (
            <>
                {children}
            </>
        );
    else
        return (
            <>
                Initializing database
            </>
        );
}