import type { IAccountObject } from "../../../Core/Data/IndexedDatabase/HintKeep/Model/IAccountObject";
import type { IFormHandler } from "../../../Core/FormHandlers/IFormHandler";
import type { AccountForm } from "../Forms/AccountForm";
import type { IAccountDetails } from "../Models/IAcountDetails";
import type { IDependencyResolver } from "react-model-view-viewmodel";
import { IndexedDatabase, mapDbRequestToPromise } from "../../../Core/Data/IndexedDatabase";
import { type IUser, User } from "../../../Core/Models";

export class AccountArchivalFormHandler implements IFormHandler<AccountForm, IAccountDetails> {
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

            if (form.id !== null) {
                const accountObject: IAccountObject = {
                    id: form.id,
                    userId: this._user.id,
                    status: "archived",
                    name: form.name.initialValue,
                    username: form.username.initialValue,
                    hint: form.hint.initialValue,
                    isPinned: form.pinned.initialValue,
                    notes: form.notes.initialValue
                };

                await mapDbRequestToPromise(accountsStore.put(accountObject));
            }
            transaction.commit();

            return {
                id: form.id || "",
                name: form.name.value,
                username: form.username.value,
                hint: form.hint.value,
                isPinned: form.pinned.value,
                notes: form.notes.value,
                isArchived: true
            };
        }
        catch (error) {
            transaction.abort();
            throw error;
        }
    }
}