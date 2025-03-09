import type { IIndexedDatabase } from "./IIndexedDatabase";
import { createContext } from "react";

export const IndexedDatabaseContext = createContext<IIndexedDatabase | null>(null);