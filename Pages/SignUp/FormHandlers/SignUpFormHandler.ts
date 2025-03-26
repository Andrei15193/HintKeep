import type { IUserObject } from "../../../Data/IndexedDatabase/HintKeep/Model/IUserObject";
import type { IFormHandler } from "../../../FormHandlers/IFormHandler";
import type { IUser } from "../../Model/IUser";
import type { SignUpForm } from "../Forms/SignUpForm";
import type { IDependencyResolver } from "react-model-view-viewmodel";
import { getHashAsync } from "../../../Crypto";
import { IndexedDatabase, mapDbRequestToPromise } from "../../../Data/IndexedDatabase";

export class SignUpFormHandler implements IFormHandler<SignUpForm, IUser | null> {
    private readonly _database: IDBDatabase;

    public constructor({ resolve }: IDependencyResolver) {
        this._database = resolve(IndexedDatabase);
    }

    public async handleAsync(form: SignUpForm): Promise<IUser | null> {
        const userObject: IUserObject = {
            username: form.username.value,
            passwordHash: await getHashAsync(form.password.value, "SHA-256"),
            hint: form.hint.value
        };

        const transaction = this._database.transaction("Users", "readwrite");
        try {
            const usersStore = transaction.objectStore("Users");

            if (await mapDbRequestToPromise(usersStore.getKey(userObject.username))) {
                transaction.abort();
                form.username.error = "The username you have picked is already in use";

                return null;
            }
            else {
                await mapDbRequestToPromise(usersStore.add(userObject));
                transaction.commit();

                return {
                    username: userObject.username
                };
            }
        }
        catch (error) {
            transaction.abort();
            throw error;
        }
    }
}