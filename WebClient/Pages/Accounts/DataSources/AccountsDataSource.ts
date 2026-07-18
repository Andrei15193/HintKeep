import type { IAccountSummaryObject } from "../../../Core/Data/IndexedDatabase/HintKeep/Model/IAccountObject";
import type { IDataSource } from "../../../Core/DataSources";
import type { IUser } from "../../../Core/Models";
import type { IAccountListItem } from "../Models/IAccountListItem";
import type { IDependencyResolver } from "react-model-view-viewmodel";
import { CurrentUser } from "../../../Core/Authentication";
import { IndexedDatabase, mapDbRequestToPromise } from "../../../Core/Data/IndexedDatabase";

export interface ISearchText {
    readonly searchText: string;
}

export interface IStatus {
    readonly status: "active" | "archvied";
}

export interface IListResult<TItem> {
    readonly items: readonly TItem[];
    readonly totalCount: number;
}

export class AccountsDataSource implements IDataSource<ISearchText & IStatus, IListResult<IAccountListItem>> {
    private readonly _user: IUser;
    private readonly _database: IDBDatabase;

    public constructor({ resolve }: IDependencyResolver) {
        this._user = resolve(CurrentUser);
        this._database = resolve(IndexedDatabase);
    }

    public async getDataAsync({ searchText, status }: ISearchText & IStatus): Promise<IListResult<IAccountListItem>> {
        const transaction = this._database.transaction("Accounts", "readonly");

        try {
            const userAccountsIndex = transaction
                .objectStore("Accounts")
                .index("UserAccountsStatus");

            const accountObjects = await mapDbRequestToPromise<readonly IAccountSummaryObject[]>(
                userAccountsIndex.getAll([
                    this._user.id,
                    status === "active" ? "active" : "archived"
                ])
            );
            transaction.commit();

            const searchTerms = searchText
                .trim()
                .split(/[^a-z0-9]/ig)
                .map((searchTerm) => searchTerm.toLowerCase());

            return {
                items: accountObjects
                    .map<IAccountListItem>((accountObject) => ({
                        id: accountObject.id,
                        name: accountObject.name,
                        username: accountObject.username,
                        hint: accountObject.hint,
                        isPinned: accountObject.isPinned
                    }))
                    .filter((account) => (
                        searchTerms.length === 0
                        || searchTerms.some((searchTerm) => (
                            account.name.includes(searchTerm)
                            || account.name.toLowerCase()
                                .includes(searchTerm)
                        ))
                    ))
                    .sort((left, right) => {
                        if (left.isPinned === right.isPinned)
                            return left.name.localeCompare(right.name, undefined, { sensitivity: "base" });
                        else if (left.isPinned)
                            return -1;
                        else
                            return 1;
                    }),
                totalCount: accountObjects.length
            };
        }
        catch (error) {
            transaction.abort();
            throw error;
        }
    }
}