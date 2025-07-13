import type { IAccountObject } from "../../../Core/Data/IndexedDatabase/HintKeep/Model/IAccountObject";
import type { IDataSource } from "../../../Core/DataSources";
import type { IAccountDetails } from "../Models/IAcountDetails";
import type { IDependencyResolver } from "react-model-view-viewmodel";
import { IndexedDatabase, mapDbRequestToPromise } from "../../../Core/Data/IndexedDatabase";

export interface IEntityScoped {
    readonly id: string;
}

export class AccountDetailsDataSource implements IDataSource<IEntityScoped, IAccountDetails> {
    private readonly _database: IDBDatabase;

    public constructor({ resolve }: IDependencyResolver) {
        this._database = resolve(IndexedDatabase);
    }

    public async getDataAsync({ id }: IEntityScoped): Promise<IAccountDetails> {
        const transaction = this._database.transaction("Accounts", "readonly");

        try {
            const userAccountsStore = transaction
                .objectStore("Accounts");

            const accountObject = await mapDbRequestToPromise<IAccountObject>(userAccountsStore.get(id));
            transaction.commit();

            return {
                id: accountObject.id,
                isArchived: accountObject.status === "archived",
                name: accountObject.name,
                username: accountObject.username,
                hint: accountObject.hint,
                isPinned: accountObject.isPinned,
                notes: accountObject.notes
            };
        }
        catch (error) {
            transaction.abort();
            throw error;
        }
    }
}