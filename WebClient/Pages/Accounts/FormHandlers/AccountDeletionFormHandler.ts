import type { AccountObjectStoreKey } from "../../../Core/Data/IndexedDatabase/HintKeep";
import type { AccountObjectType } from "../../../Core/Data/IndexedDatabase/HintKeep/Model/IAccountObject";
import type { IFormHandler } from "../../../Core/FormHandlers/IFormHandler";
import type { IUser } from "../../../Core/Models";
import type { AccountForm } from "../Forms/AccountForm";
import type { IAccountDetails } from "../Models/IAcountDetails";
import type { IDependencyResolver } from "react-model-view-viewmodel";
import { CurrentUser } from "../../../Core/Authentication";
import { IndexedDatabase, mapDbRequestToPromise } from "../../../Core/Data/IndexedDatabase";

export class AccountDeletionFormHandler implements IFormHandler<AccountForm, IAccountDetails> {
    private readonly _user: IUser;
    private readonly _database: IDBDatabase;

    public constructor({ resolve }: IDependencyResolver) {
        this._user = resolve(CurrentUser);
        this._database = resolve(IndexedDatabase);
    }

    public async handleAsync(form: AccountForm): Promise<IAccountDetails> {
        const transaction = this._database.transaction("Accounts", "readwrite");

        try {
            const userAccountsStore = transaction.objectStore("Accounts");
            if (form.id !== null) {
                await mapDbRequestToPromise(userAccountsStore.delete([this._user.id, form.id, "summary"] satisfies AccountObjectStoreKey));

                const userAccountHintsIndex = userAccountsStore
                    .index("AccountHints");
                const accountHintObjects = await mapDbRequestToPromise<readonly AccountObjectStoreKey[]>(
                    userAccountHintsIndex.getAllKeys([
                        this._user.id,
                        form.id,
                        "hint" satisfies AccountObjectType
                    ])
                );
                for (let [,hintId] of accountHintObjects)
                    await mapDbRequestToPromise(userAccountsStore.delete([this._user.id, hintId, "hint"] satisfies AccountObjectStoreKey));
            }
            transaction.commit();

            return {
                id: form.id || "",
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