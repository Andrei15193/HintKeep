import type { AccountObjectType, IAccountHintObject, IAccountSummaryObject } from "../../../Core/Data/IndexedDatabase/HintKeep/Model/IAccountObject";
import type { IDataSource } from "../../../Core/DataSources";
import type { IUser } from "../../../Core/Models";
import type { IAccountHintListItem } from "../Models/IAccountHintListItem";
import type { IDependencyResolver } from "react-model-view-viewmodel";
import { CurrentUser } from "../../../Core/Authentication";
import { IndexedDatabase, mapDbRequestToPromise } from "../../../Core/Data/IndexedDatabase";

export interface IAccountScoped {
    readonly accountId: string;
}

export interface IStatus {
    readonly status: "active" | "archvied";
}

export interface IListResult<TItem> {
    readonly account: {
        readonly id: string;
        readonly name: string;
    };
    readonly items: readonly TItem[];
    readonly totalCount: number;
}

export class AccountHintsDataSource implements IDataSource<IAccountScoped, IListResult<IAccountHintListItem>> {
    private readonly _user: IUser;
    private readonly _database: IDBDatabase;

    public constructor({ resolve }: IDependencyResolver) {
        this._user = resolve(CurrentUser);
        this._database = resolve(IndexedDatabase);
    }

    public async getDataAsync({ accountId }: IAccountScoped): Promise<IListResult<IAccountHintListItem>> {
        const transaction = this._database.transaction("Accounts", "readonly");

        try {
            const userAccountsStore = transaction
                .objectStore("Accounts");
            const userAccountHintsIndex = userAccountsStore
                .index("AccountHints");

            const accountObject = await mapDbRequestToPromise<IAccountSummaryObject>(
                userAccountsStore.get([
                    this._user.id,
                    accountId,
                    "summary" satisfies AccountObjectType
                ])
            );
            const accountHintObjects = await mapDbRequestToPromise<readonly IAccountHintObject[]>(
                userAccountHintsIndex.getAll([
                    this._user.id,
                    accountId,
                    "hint" satisfies AccountObjectType
                ])
            );
            transaction.commit();

            return {
                account: {
                    id: accountObject.id,
                    name: accountObject.name
                },
                items: accountHintObjects
                    .map<IAccountHintListItem>((accountHintObject) => ({
                        id: accountHintObject.id,
                        hint: accountHintObject.hint,
                        dateAdded: new Date(accountHintObject.dateAdded)
                    }))
                    .sort((left, right) => {
                        if (left.dateAdded < right.dateAdded)
                            return 1;
                        else if (left.dateAdded > right.dateAdded)
                            return -1;
                        else
                            return 0;
                    }),
                totalCount: accountHintObjects.length
            };
        }
        catch (error) {
            transaction.abort();
            throw error;
        }
    }
}