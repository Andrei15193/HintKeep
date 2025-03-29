import type { IAccountObject } from "../../../Data/IndexedDatabase/HintKeep/Model/IAccountObject";
import type { IFormHandler } from "../../../FormHandlers/IFormHandler";
import type { IAccount } from "../../Model/IAccount";
import type { AccountForm } from "../Forms/AccountForm";
import type { IDependencyResolver } from "react-model-view-viewmodel";
import { IndexedDatabase, mapDbRequestToPromise } from "../../../Data/IndexedDatabase";

export class AddAccountFormHandler implements IFormHandler<AccountForm, IAccount> {
    private readonly _database: IDBDatabase;

    public constructor({ resolve }: IDependencyResolver) {
        this._database = resolve(IndexedDatabase);
    }

    public async handleAsync(form: AccountForm): Promise<IAccount> {
        const transaction = this._database.transaction("Accounts", "readwrite");

        try {
            const accountsStore = transaction.objectStore("Accounts");

            let accountId: string;
            do {
                accountId = crypto.randomUUID();
            } while (await mapDbRequestToPromise(accountsStore.getKey(accountId)));

            const accountObject: IAccountObject = {
                id: accountId,
                name: form.name.value,
                username: form.username.value,
                hint: form.hint.value,
                isPinned: form.pinned.value,
                notes: form.notes.value
            };

            await mapDbRequestToPromise(accountsStore.add(accountObject));
            transaction.commit();

            return {
                id: accountId,
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