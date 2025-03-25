import React from "react";
import { Link } from "react-router";
import { useIndexedDatabase } from "../../Data/IndexedDatabase";
import { useCreateFlow } from "../../PageFlows";
import { TextInput } from "../Forms";
import { LoginFormHandler } from "./FormHandlers/LoginFormHandler";
import { LoginForm } from "./Forms/LoginForm";

export function LoginPage(): React.JSX.Element {
    const { closeDatabase } = useIndexedDatabase();

    const {
        form,
        submitAsync
    } = useCreateFlow({
        form: LoginForm,
        formHandler: LoginFormHandler
    });

    return (
        <>
            <h2>
                Login
            </h2>

            <p>
                Welcome to HintKeep, please provide your credentials to start using the app!
            </p>

            <form onSubmit={submitAsync}>
                {
                    form.isInvalid
                        ? (
                            <div>
                                {form.error}
                            </div>
                        )
                        : null
                }

                <div>
                    <label htmlFor={form.username.name}>
                        Username
                    </label>
                    <TextInput
                        field={form.username}
                        isInvalid={form.error !== null}
                    />
                </div>

                <div>
                    <label htmlFor={form.password.name}>
                        Password
                    </label>
                    <TextInput
                        field={form.password}
                        type="password"
                        isInvalid={form.error !== null}
                    />
                </div>

                <button type="submit">
                    Login
                </button>

                <Link
                    to="/"
                    onClick={closeDatabase}
                >
                    Cancel
                </Link>
            </form>

            <div>
                <a
                    href="#"
                    title="Currently unavailable"
                >
                    Account recovery
                </a>
            </div>

            <div>
                <Link to="/sign-up">
                    Sign up
                </Link>
            </div>
        </>
    );
}