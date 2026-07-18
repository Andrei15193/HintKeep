import type { AccountObjectType, IAccountHintObject, IAccountSummaryObject } from "../../../Core/Data/IndexedDatabase/HintKeep/Model/IAccountObject";
import type { IFormHandler } from "../../../Core/FormHandlers/IFormHandler";
import type { IUser } from "../../../Core/Models";
import type { AccountForm } from "../Forms/AccountForm";
import type { IAccountDetails } from "../Models/IAcountDetails";
import type { IDependencyResolver } from "react-model-view-viewmodel";
import { CurrentUser } from "../../../Core/Authentication";
import { IndexedDatabase, mapDbRequestToPromise } from "../../../Core/Data/IndexedDatabase";

export class AccountFormHandler implements IFormHandler<AccountForm, IAccountDetails> {
    private readonly _user: IUser;
    private readonly _database: IDBDatabase;

    public constructor({ resolve }: IDependencyResolver) {
        this._user = resolve(CurrentUser);
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
                while (await mapDbRequestToPromise(accountsStore.getKey([accountId, "summary" satisfies AccountObjectType])));

            let hintId: string;
            do
                hintId = crypto.randomUUID();
            while (await mapDbRequestToPromise(accountsStore.getKey([hintId, "hint" satisfies AccountObjectType])));

            const accountObject: IAccountSummaryObject = {
                userId: this._user.id,
                id: accountId,
                type: "summary",

                status: form.archived ? "archived" : "active",
                name: form.name.value,
                username: form.username.value,
                hint: form.hint.value,
                isPinned: form.pinned.value,
                notes: form.notes.value
            };

            await mapDbRequestToPromise(accountsStore.put(accountObject));

            if (form.hint.hasChanged) {
                const accountHintObject: IAccountHintObject = {
                    userId: this._user.id,
                    id: hintId,
                    type: "hint",

                    accountId: accountId,
                    hint: form.hint.value,
                    dateAdded: new Date()
                        .toISOString()
                };
                await mapDbRequestToPromise(accountsStore.put(accountHintObject));
            }
            transaction.commit();

            return {
                id: accountId,
                name: form.name.value,
                username: form.username.value,
                hint: form.hint.value,
                isPinned: form.pinned.value,
                notes: form.notes.value,
                isArchived: form.archived
            };
        }
        catch (error) {
            transaction.abort();
            throw error;
        }
    }
}