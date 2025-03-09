import type { IIndexedDatabase } from "./IIndexedDatabase";
import type { IIndexedDatabaseConnectionInfo } from "./IIndexedDatabaseConnectionInfo";
import React, { type PropsWithChildren, useRef, useState, useCallback, useMemo } from "react";
import { IndexedDatabaseContext } from "./IndexedDatabaseContext";

export interface IIndexedDatabaseProviderProps {
    readonly connectionInfo: IIndexedDatabaseConnectionInfo;
}

export function IndexedDatabaseProvider({ connectionInfo: { name, version, databaseMigrations }, children }: PropsWithChildren<IIndexedDatabaseProviderProps>): React.JSX.Element {
    const databaseRef = useRef<IDBDatabase | null>(null);
    const errorRef = useRef<unknown | null>(null);
    const [state, setState] = useState<IIndexedDatabase["state"]>("uninitialized");

    const initializeAsyncCallback = useCallback(
        () => new Promise<void>((resolve) => {
            try {
                setState("opening");

                const databaseRequest = window.indexedDB.open(name, version);

                databaseRequest.addEventListener("upgradeneeded", ({ oldVersion, newVersion }) => {
                    const database = databaseRequest.result;
                    const sortedDatabaseMigrations = databaseMigrations
                        .slice()
                        .sort((left, right) => left.version - right.version);

                    const migrationStartIndex = sortedDatabaseMigrations.findIndex((migration) => migration.version > oldVersion);
                    if (migrationStartIndex >= 0)
                        for (
                            let migrationIndex = migrationStartIndex;
                            migrationIndex < sortedDatabaseMigrations.length && (
                                newVersion === null || sortedDatabaseMigrations[migrationIndex]!.version <= newVersion
                            );
                            migrationIndex++
                        )
                            sortedDatabaseMigrations[migrationIndex]!.applyChanges(database);
                });

                databaseRequest.addEventListener("success", (event) => {
                    databaseRef.current = databaseRequest.result;
                    setState("ready");
                    resolve();
                });

                databaseRequest.addEventListener("error", () => {
                    errorRef.current = databaseRequest.error;
                    setState("unavailable");
                    resolve();
                });
            }
            catch (error) {
                errorRef.current = error;
                setState("unavailable");
            }
        }),
        [name, version, databaseMigrations]
    );

    const indexedDatabase = useMemo<IIndexedDatabase>(
        () => ({
            state,

            database: databaseRef.current,
            error: errorRef.current,

            isUninitialized: (state === "uninitialized") as any,
            isOpening: (state === "opening") as any,
            isReady: (state === "ready") as any,
            isUnavailable: (state === "unavailable") as any,

            initializeAsync: initializeAsyncCallback
        }),
        [state, databaseRef, errorRef, initializeAsyncCallback]
    );

    return (
        <IndexedDatabaseContext.Provider
            value={indexedDatabase}
            children={children}
        />
    );
}