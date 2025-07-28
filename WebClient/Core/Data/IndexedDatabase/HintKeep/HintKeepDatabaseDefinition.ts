import type { IIndexedDatabaseDefinition } from "../IIndexedDatabaseDefinition";
import type { AccountObjectType, IAccountObject } from "./Model/IAccountObject";

export type AccountObjectStoreKey = readonly [userId: string, id: string, type: AccountObjectType];

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
                        keyPath: ["userId", "id", "type"] satisfies (keyof IAccountObject)[]
                    })
                    .createIndex("UserAccounts", "userId", {
                        unique: false
                    })
                    .objectStore
                    .createIndex("UserAccountsStatus", ["userId", "status"], {
                        unique: false
                    })
                    .objectStore
                    .createIndex("AccountHints", ["userId", "accountId", "type"], {
                        unique: false
                    });
            }
        }
    ]
};