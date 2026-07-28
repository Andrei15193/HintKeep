import type { IIndexedDatabase } from "./IIndexedDatabase";
import { createContext } from "react";

/** @deprecated switch to {@link IIndexedDatabaseProvider} */
export const IndexedDatabaseContext = createContext<IIndexedDatabase | null>(null);