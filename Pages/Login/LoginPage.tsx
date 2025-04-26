import React, { useEffect } from "react";
import { Link, useNavigate } from "react-router";
import { useCreateFlow } from "../../PageFlows";
import { useUserContext } from "../Contexts/UserContext";
import { TextInput } from "../Forms";
import { LoginFormHandler } from "./FormHandlers/LoginFormHandler";
import { LoginForm } from "./Forms/LoginForm";

export function LoginPage(): React.JSX.Element {
    const navigate = useNavigate();
    const { authenticate } = useUserContext();

    const {
        form,
        isSubmitted,
        result: user,
        submitAsync
    } = useCreateFlow({
        form: LoginForm,
        formHandler: LoginFormHandler,
        skipConfirmationPrompt: true
    });

    useEffect(
        () => {
            if (isSubmitted) {
                authenticate(user!);
                navigate("/");
            }
        },
        [isSubmitted, user, authenticate, navigate]
    );

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

                <Link to="/">
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