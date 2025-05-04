import type { IIndexedDatabaseDefinition } from "../IIndexedDatabaseDefinition";

export const HintKeepDatabaseDefinition: IIndexedDatabaseDefinition = {
    name: "HintKeep",

    structureDefinitions: [
        {
            configure(database) {
                database
                    .createObjectStore("Users", {
                        keyPath: "id"
                    })
                    .createIndex("Usernames", "username", {
                        unique: true
                    })
                    .objectStore
                    .createIndex("Authenticaiton", ["username", "passwordHash"], {
                        unique: false
                    });

                database
                    .createObjectStore("Accounts", {
                        keyPath: "id"
                    })
                    .createIndex("UserAccounts", "userId", {
                        unique: false
                    });
            }
        }
    ]
};