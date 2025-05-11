import type { IFormHandler } from "../../../Core/FormHandlers/IFormHandler";
import type { AccountForm } from "../Forms/AccountForm";
import type { IAccountDetails } from "../Models/IAcountDetails";
import type { IDependencyResolver } from "react-model-view-viewmodel";
import { IndexedDatabase, mapDbRequestToPromise } from "../../../Core/Data/IndexedDatabase";

export class AccountDeletionFormHandler implements IFormHandler<AccountForm, IAccountDetails> {
    private readonly _database: IDBDatabase;

    public constructor({ resolve }: IDependencyResolver) {
        this._database = resolve(IndexedDatabase);
    }

    public async handleAsync(form: AccountForm): Promise<IAccountDetails> {
        const transaction = this._database.transaction("Accounts", "readwrite");

        try {
            const accountsStore = transaction.objectStore("Accounts");
            if (form.id !== null)
                await mapDbRequestToPromise(accountsStore.delete(form.id));
            transaction.commit();

            return {
                id: form.id || "",
                name: form.name.value,
                username: form.username.value,
                hint: form.hint.value,
                isPinned: form.pinned.value,
                notes: form.notes.value
            };
        }
        catch (error) {
            transaction.abort();
            throw error;
        }
    }
}