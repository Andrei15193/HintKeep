import type { IUserObject } from "../../../Core/Data/IndexedDatabase/HintKeep/Model/IUserObject";
import type { IFormHandler } from "../../../Core/FormHandlers/IFormHandler";
import type { IUser } from "../../../Core/Models";
import type { LoginForm } from "../Forms/LoginForm";
import type { IDependencyResolver } from "react-model-view-viewmodel";
import { getHashAsync } from "../../../Core/Crypto";
import { type IIndexedDatabaseProvider, IndexedDatabaseProvider, mapDbRequestToPromise } from "../../../Core/Data/IndexedDatabase";

export class IndexedDbLoginFormHandler implements IFormHandler<LoginForm, IUser> {
    private readonly _indexedDatabaseProvider: IIndexedDatabaseProvider;

    public constructor({ resolve }: IDependencyResolver) {
        this._indexedDatabaseProvider = resolve(IndexedDatabaseProvider);
    }

    public async handleAsync(form: LoginForm): Promise<IUser | null> {
        const database = await this._indexedDatabaseProvider.openAsync();

        const passwordHash = await getHashAsync(form.password.value, "SHA-256");

        const transaction = database.transaction("Users", "readonly");
        try {
            const usersAuthenticationIndex = transaction
                .objectStore("Users")
                .index("Authenticaiton");

            const userObject = await mapDbRequestToPromise<IUserObject | undefined>(usersAuthenticationIndex.get([form.username.value.toLowerCase(), passwordHash]));
            transaction.commit();

            if (userObject) {
                return {
                    id: userObject.id,
                    username: userObject.username
                };
            }
            else {
                form.username.error = "Wrong credentials. Try again or follow the password recovery steps.";

                return null;
            }
        }
        catch (error) {
            transaction.abort();
            throw error;
        }
    }
}