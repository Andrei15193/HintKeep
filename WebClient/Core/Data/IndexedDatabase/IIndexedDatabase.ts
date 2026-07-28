import { DependencyToken } from "react-model-view-viewmodel";

/** @deprecated switch to {@link IIndexedDatabaseProvider} */
export const IndexedDatabase = new DependencyToken<IDBDatabase>("IndexedDatabase");

/** @deprecated switch to {@link IIndexedDatabaseProvider} */
export interface IIndexedDatabase {
    readonly state: "uninitialized" | "opening" | "ready" | "unavailable" | "closed";

    readonly database: IDBDatabase | null;
    readonly error: unknown | null;

    initializeAsync(): Promise<void>;
    closeDatabase(): void;

    readonly isUninitialized: boolean;
    readonly isOpening: boolean;
    readonly isReady: boolean;
    readonly isUnavailable: boolean;
    readonly isClosed: boolean;
}