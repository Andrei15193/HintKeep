import React, { type FormEvent, useCallback } from "react";
import { useViewModel } from "react-model-view-viewmodel";
import { useIndexedDatabase } from "../../Data/IndexedDatabase";
import { TextInput } from "../Forms";
import { LoginForm } from "./Forms/LoginForm";

export function Login(): React.JSX.Element {
    const form = useViewModel(LoginForm);
    const { initializeAsync } = useIndexedDatabase();

    const onSubmitCallback = useCallback(
        (event: FormEvent<HTMLFormElement>) => {
            event.preventDefault();

            form.fields.forEach((field) => field.wasTouched = true);
            form.validation.validate();

            if (form.isValid)
                initializeAsync()
                    .then(
                        () => {
                            form.username.value = "";
                            form.password.value = "";
                        }
                    );
        },
        [form, initializeAsync]
    );

    return (
        <>
            <h2>
                Login
            </h2>

            <form onSubmit={onSubmitCallback}>
                <div>
                    <label htmlFor={form.username.name}>
                        Username
                    </label>
                    <TextInput field={form.username} />
                </div>

                <div>
                    <label htmlFor={form.password.name}>
                        Password
                    </label>
                    <TextInput field={form.password} />
                </div>

                <button type="submit">
                    Login
                </button>
            </form>
        </>
    );
}