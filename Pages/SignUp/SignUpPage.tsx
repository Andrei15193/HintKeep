import React, { useEffect } from "react";
import { Link, useNavigate } from "react-router";
import { useCreateFlow } from "../../PageFlows";
import { useUserContext } from "../Contexts/UserContext";
import { TextArea, TextInput } from "../Forms";
import { SignUpFormHandler } from "./FormHandlers/SignUpFormHandler";
import { SignUpForm } from "./Forms/SignUpForm";

export function SignUpPage(): React.JSX.Element {
    const navigate = useNavigate();
    const { authenticate } = useUserContext();
    const {
        form,
        isProcessing,
        isSubmitted,
        result: user,
        submitAsync
    } = useCreateFlow({
        form: SignUpForm,
        formHandler: SignUpFormHandler
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
                Sign Up
            </h2>

            <p>
                Welcome to HintKeep, please provide the following information to start using the app!
            </p>

            {
                isProcessing
                    ? "Loading"
                    : (
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

                            <div>
                                <label htmlFor={form.hint.name}>
                                    Hint
                                </label>
                                <TextArea
                                    field={form.hint}
                                    isInvalid={form.error !== null}
                                />
                            </div>

                            <button type="submit">
                                Sign Up
                            </button>

                            <Link to="/login">
                                Cancel
                            </Link>
                        </form>
                    )
            }
        </>
    );
}