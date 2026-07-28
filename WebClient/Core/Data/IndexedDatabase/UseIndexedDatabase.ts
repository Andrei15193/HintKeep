import type { IIndexedDatabase } from "./IIndexedDatabase";
import type { IIndexedDatabaseProvider } from "./IndexedDatabaseProvider";
import { useContext } from "react";
import { IndexedDatabaseContext } from "./IndexedDatabaseContext";

/** @deprecated switch to {@link IIndexedDatabaseProvider} */
export function useIndexedDatabase(): IIndexedDatabase {
    return useContext(IndexedDatabaseContext)!;
}