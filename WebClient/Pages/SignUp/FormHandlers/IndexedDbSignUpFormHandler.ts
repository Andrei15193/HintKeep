import type { IUserObject } from "../../../Core/Data/IndexedDatabase/HintKeep/Model/IUserObject";
import type { IFormHandler } from "../../../Core/FormHandlers/IFormHandler";
import type { IUser } from "../../../Core/Models";
import type { SignUpForm } from "../Forms/SignUpForm";
import type { IDependencyResolver } from "react-model-view-viewmodel";
import { getHashAsync } from "../../../Core/Crypto";
import { type IIndexedDatabaseProvider, IndexedDatabaseProvider, mapDbRequestToPromise } from "../../../Core/Data/IndexedDatabase";

export class IndexedDbSignUpFormHandler implements IFormHandler<SignUpForm, IUser | null> {
    private readonly _indexedDatabaseProvider: IIndexedDatabaseProvider;

    public constructor({ resolve }: IDependencyResolver) {
        this._indexedDatabaseProvider = resolve(IndexedDatabaseProvider);
    }

    public async handleAsync(form: SignUpForm): Promise<IUser | null> {
        const database = await this._indexedDatabaseProvider.openAsync();
        const passwordHash = await getHashAsync(form.password.value, "SHA-256");

        const transaction = database.transaction("Users", "readwrite");
        try {
            const usersStore = transaction.objectStore("Users");

            let userId: string;
            do
                userId = crypto.randomUUID();
            while (await mapDbRequestToPromise(usersStore.getKey(userId)));

            const userObject: IUserObject = {
                id: userId,
                username: form.username.value.toLowerCase(),
                passwordHash: passwordHash,
                hint: form.hint.value
            };

            await mapDbRequestToPromise(usersStore.add(userObject));
            transaction.commit();

            return {
                id: userObject.id,
                username: userObject.username
            };
        }
        catch (error) {
            transaction.abort();
            if (error instanceof DOMException && error.name === "ConstraintError") {
                form.username.error = "Duplicate account";

                return null;
            }
            else
                throw error;
        }
    }
}