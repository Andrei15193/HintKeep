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
        const passwordHash = await getHashAsync(form.password.value, "SHA-256");

        const transaction = this._database.transaction("Users", "readwrite");
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
                form.error = "Duplicate account";

                return null;
            }
            else
                throw error;
        }
    }
}