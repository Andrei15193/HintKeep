import type { IUserObject } from "../../../Data/IndexedDatabase/HintKeep/Model/IUserObject";
import type { IFormHandler } from "../../../FormHandlers/IFormHandler";
import type { IUser } from "../../Model/IUser";
import type { LoginForm } from "../Forms/LoginForm";
import type { IDependencyResolver } from "react-model-view-viewmodel";
import { getHashAsync } from "../../../Crypto";
import { IndexedDatabase, mapDbRequestToPromise } from "../../../Data/IndexedDatabase";

export class LoginFormHandler implements IFormHandler<LoginForm, IUser | null> {
    private readonly _database: IDBDatabase;

    public constructor({ resolve }: IDependencyResolver) {
        this._database = resolve(IndexedDatabase);
    }

    public async handleAsync(form: LoginForm): Promise<IUser | null> {
        const passwordHash = await getHashAsync(form.password.value, "SHA-256");

        const transaction = this._database.transaction("Users", "readonly");
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
                form.error = "Wrong credentials. Try again or follow the password recovery steps.";

                return null;
            }
        }
        catch (error) {
            transaction.abort();
            throw error;
        }
    }
}