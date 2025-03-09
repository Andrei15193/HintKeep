export interface IIndexedDatabase {
    readonly state: "uninitialized" | "opening" | "ready" | "unavailable";

    readonly database: IDBDatabase | null;
    readonly error: unknown | null;

    initializeAsync(): Promise<void>;

    readonly isUninitialized: this["state"] extends "uninitialized" ? true : false;
    readonly isOpening: this["state"] extends "opening" ? true : false;
    readonly isReady: this["state"] extends "ready" ? true : false;
    readonly isUnavailable: this["state"] extends "unavailable" ? true : false;
}