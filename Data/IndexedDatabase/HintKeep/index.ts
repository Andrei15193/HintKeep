import type { IIndexedDatabaseConnectionInfo } from "../IIndexedDatabaseConnectionInfo";

export const HintKeepDatabaseConnectionInfo: IIndexedDatabaseConnectionInfo = {
    name: "HintKeep",
    get version() {
        return Math.max(...this.databaseMigrations.map(({ version }) => version));
    },

    databaseMigrations: [
        {
            version: 1,

            applyChanges(database) {
                database.createObjectStore("Users", {
                    keyPath: "Username"
                });
            },

            revertChanges(database) {
                database.deleteObjectStore("Users");
            }
        }
    ]
};