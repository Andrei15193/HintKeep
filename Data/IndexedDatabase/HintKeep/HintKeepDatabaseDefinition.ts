import type { IIndexedDatabaseDefinition } from "../IIndexedDatabaseDefinition";

export const HintKeepDatabaseDefinition: IIndexedDatabaseDefinition = {
    name: "HintKeep",

    structureDefinitions: [
        {
            configure(database) {
                database.createObjectStore("Users", {
                    keyPath: "username"
                });
            }
        },
        {
            configure(database) {
                database.createObjectStore("Accounts", {
                    keyPath: "id"
                });
            }
        }
    ]
};