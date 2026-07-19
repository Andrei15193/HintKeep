import type { IIndexedDatabase } from "./IIndexedDatabase";
import type { IIndexedDatabaseDefinition } from "./IIndexedDatabaseDefinition";
import React, { type PropsWithChildren, useRef, useState, useCallback, useMemo } from "react";
import { DependencyToken } from "react-model-view-viewmodel";
import { useWindow } from "../../../Pages/WindowContext";
import { IndexedDatabaseContext } from "./IndexedDatabaseContext";
import { mapDbRequestToPromise } from "./MapDbRequestToPromise";

export const IndexedDatabaseProvider = new DependencyToken<IIndexedDatabaseProvider>("indexed database provider");

export interface IIndexedDatabaseProvider {
    openDatabaseAsync(): Promise<IDBDatabase>;
}

export const IndexedDatabaseHandler = new DependencyToken<IIndexedDatabaseHandler>("indexed database handler");
export interface IIndexedDatabaseHandler extends IIndexedDatabaseProvider {
    dropDatabaseAsync(): Promise<void>;
}

export class IndexedDatabaseHandlerService implements IIndexedDatabaseHandler {
    private _databasePromise: Promise<IDBDatabase> | null = null;
    private readonly _databaseDefinition: IIndexedDatabaseDefinition;
    private readonly _window: Window;

    public constructor(databaseDefinition: IIndexedDatabaseDefinition, window: Window) {
        this._databaseDefinition = databaseDefinition;
        this._window = window;
    }

    public openDatabaseAsync(): Promise<IDBDatabase> {
        if (this._databasePromise === null)
            this._databasePromise = new Promise<IDBDatabase>((resolve, reject) => {
                try {
                    const { name, structureDefinitions } = this._databaseDefinition;
                    const databaseRequest = this._window.indexedDB.open(name, structureDefinitions.length);

                    databaseRequest.addEventListener("upgradeneeded", ({ oldVersion, newVersion }) => {
                        for (let databaseStructureChangeIndex = oldVersion; databaseStructureChangeIndex < newVersion!; databaseStructureChangeIndex++)
                            structureDefinitions[databaseStructureChangeIndex]!.configure(databaseRequest.result);
                    });

                    databaseRequest.addEventListener("success", () => {
                        resolve(databaseRequest.result);
                    });

                    databaseRequest.addEventListener("error", () => {
                        reject(databaseRequest.error);
                    });
                }
                catch (error) {
                    reject(error);
                }
            });

        return this._databasePromise;
    }

    public async dropDatabaseAsync(): Promise<void> {
        const { name } = this._databaseDefinition;
        const databases = await this._window.indexedDB.databases();

        try {
            await Promise.all(
                databases
                    .filter((database) => database.name !== undefined && database.name.localeCompare(name, "en-GB", { sensitivity: "base" }) >= 0)
                    .map((database) => mapDbRequestToPromise(this._window.indexedDB.deleteDatabase(database.name!)))
            );
        }
        finally {
            this._databasePromise = null;
        }
    }
}

export interface IIndexedDatabaseProviderProps {
    readonly databaseDefinition: IIndexedDatabaseDefinition;
}

export function IndexedDatabaseContextProvider({ databaseDefinition: { name, structureDefinitions }, children }: PropsWithChildren<IIndexedDatabaseProviderProps>): React.JSX.Element {
    const databaseRef = useRef<IDBDatabase | null>(null);
    const errorRef = useRef<unknown | null>(null);
    const { indexedDB } = useWindow();
    const [state, setState] = useState<IIndexedDatabase["state"]>("uninitialized");

    const initializeAsyncCallback = useCallback(
        () => new Promise<void>((resolve) => {
            try {
                setState("opening");

                const databaseRequest = indexedDB.open(name, structureDefinitions.length);

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
        [name, structureDefinitions, indexedDB]
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