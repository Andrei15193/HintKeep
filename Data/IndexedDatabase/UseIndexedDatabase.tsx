import type { IIndexedDatabase } from "./IIndexedDatabase";
import { useContext } from "react";
import { IndexedDatabaseContext } from "./IndexedDatabaseContext";

export function useIndexedDatabase(): IIndexedDatabase {
    return useContext(IndexedDatabaseContext)!;
}