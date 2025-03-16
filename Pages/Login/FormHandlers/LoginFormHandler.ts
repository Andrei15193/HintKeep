import type { IFormHandler } from "../../../FormHandlers/IFormHandler";
import type { LoginForm } from "../Forms/LoginForm";
import type { IDependencyResolver } from "react-model-view-viewmodel";
// import { IndexedDatabase } from "../../../Data/IndexedDatabase";

export class LoginFormHandler implements IFormHandler<LoginForm> {
    // private readonly _database: IDBDatabase;

    public constructor({ resolve }: IDependencyResolver) {
        //  this._database = resolve(IndexedDatabase);
    }

    public async handleAsync(form: LoginForm): Promise<void> {
        form.error = "Wrong credentials. Try again or follow the password recovery steps.";
    }
}