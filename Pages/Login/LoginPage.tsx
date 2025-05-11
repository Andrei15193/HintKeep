import React, { useEffect } from "react";
import { Link, useNavigate } from "react-router";
import { useAuthentication } from "../../Core/Contexts/AuthenticationContext";
import { Form, SubmitButton } from "../../Core/Forms/Components";
import { FormField, FormFieldError, FormFieldLabel, FormFieldTextInput } from "../../Core/Forms/Components/FormFields";
import { useFormFlow } from "../../Core/PageFlows";
import { LoginFormHandler } from "./FormHandlers/LoginFormHandler";
import { LoginForm } from "./Forms/LoginForm";

export function LoginPage(): React.JSX.Element {
    const navigate = useNavigate();
    const { authenticate } = useAuthentication();

    const {
        form,
        result: user,
        isSubmitting: isLoggingIn,
        isSubmitted: isLoggedIn,
        submitAsync: logInAsync
    } = useFormFlow({
        form: LoginForm,
        formHandler: LoginFormHandler
    });

    useEffect(
        () => {
            if (isLoggedIn) {
                authenticate(user);
                navigate("/");
            }
        },
        [isLoggedIn, user, authenticate, navigate]
    );

    return (
        <>
            <h2>
                Login
            </h2>

            <p>
                Welcome to HintKeep, please provide your credentials to start using the app!
            </p>

            <Form
                isLoading={isLoggingIn}
                onSubmit={logInAsync}
            >
                <FormField field={form.username}>
                    <FormFieldLabel />
                    <FormFieldTextInput />
                    <FormFieldError />
                </FormField>

                <FormField field={form.password}>
                    <FormFieldLabel />
                    <FormFieldTextInput type="password" />
                    <FormFieldError />
                </FormField>

                <SubmitButton text="Login" />

                <Link to="/">
                    Cancel
                </Link>
            </Form>

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