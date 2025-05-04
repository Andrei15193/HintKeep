import type { IIndexedDatabase } from "./IIndexedDatabase";
import type { IIndexedDatabaseDefinition } from "./IIndexedDatabaseDefinition";
import React, { type PropsWithChildren, useRef, useState, useCallback, useMemo } from "react";
import { IndexedDatabaseContext } from "./IndexedDatabaseContext";

export interface IIndexedDatabaseProviderProps {
    readonly databaseDefinition: IIndexedDatabaseDefinition;
}

export function IndexedDatabaseProvider({ databaseDefinition: { name, structureDefinitions }, children }: PropsWithChildren<IIndexedDatabaseProviderProps>): React.JSX.Element {
    const databaseRef = useRef<IDBDatabase | null>(null);
    const errorRef = useRef<unknown | null>(null);
    const [state, setState] = useState<IIndexedDatabase["state"]>("uninitialized");

    const initializeAsyncCallback = useCallback(
        () => new Promise<void>((resolve) => {
            try {
                setState("opening");

                const databaseRequest = window.indexedDB.open(name, structureDefinitions.length);

                databaseRequest.addEventListener("upgradeneeded", ({ oldVersion, newVersion }) => {
                    for (let databaseStructureChangeIndex = oldVersion; databaseStructureChangeIndex < newVersion!; databaseStructureChangeIndex++)
                        structureDefinitions[databaseStructureChangeIndex]!.configure(databaseRequest.result);
                });

                databaseRequest.addEventListener("success", () => {
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
        [name, structureDefinitions]
    );

    const closeDatabaseCallback = useCallback(
        () => {
            if (databaseRef.current !== null) {
                databaseRef.current.close();
                databaseRef.current = null;
                setState("closed");
            }
        },
        [databaseRef, setState]
    );

    const indexedDatabase = useMemo<IIndexedDatabase>(
        () => ({
            state,

            database: databaseRef.current,
            error: errorRef.current,

            isUninitialized: state === "uninitialized",
            isOpening: state === "opening",
            isReady: state === "ready",
            isUnavailable: state === "unavailable",
            isClosed: state === "closed",

            initializeAsync: initializeAsyncCallback,
            closeDatabase: closeDatabaseCallback
        }),
        [state, databaseRef, errorRef, initializeAsyncCallback, closeDatabaseCallback]
    );

    return (
        <IndexedDatabaseContext.Provider
            value={indexedDatabase}
            children={children}
        />
    );
}