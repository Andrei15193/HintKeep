import type { IAccountObject } from "../../../Data/IndexedDatabase/HintKeep/Model/IAccountObject";
import type { IFormHandler } from "../../../FormHandlers/IFormHandler";
import type { AccountForm } from "../Forms/AccountForm";
import type { IAccountDetails } from "../Models/IAcountDetails";
import type { IDependencyResolver } from "react-model-view-viewmodel";
import { IndexedDatabase, mapDbRequestToPromise } from "../../../Data/IndexedDatabase";
import { type IUser, User } from "../../Model/IUser";

export class AccountFormHandler implements IFormHandler<AccountForm, IAccountDetails> {
    private readonly _user: IUser;
    private readonly _database: IDBDatabase;

    public constructor({ resolve }: IDependencyResolver) {
        this._user = resolve(User);
        this._database = resolve(IndexedDatabase);
    }

    public async handleAsync(form: AccountForm): Promise<IAccountDetails> {
        const transaction = this._database.transaction("Accounts", "readwrite");

        try {
            const accountsStore = transaction.objectStore("Accounts");

            let accountId: string;
            if (form.id !== null)
                accountId = form.id;
            else
                do
                    accountId = crypto.randomUUID();
                while (await mapDbRequestToPromise(accountsStore.getKey(accountId)));

            const accountObject: IAccountObject = {
                userId: this._user.id,
                id: accountId,
                name: form.name.value,
                username: form.username.value,
                hint: form.hint.value,
                isPinned: form.pinned.value,
                notes: form.notes.value
            };

            await mapDbRequestToPromise(accountsStore.put(accountObject));
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