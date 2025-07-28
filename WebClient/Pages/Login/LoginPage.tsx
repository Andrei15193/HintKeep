import React, { useEffect } from "react";
import { type NonIndexRouteObject, Link, useNavigate } from "react-router";
import { useAuthentication } from "../../Core/Contexts/AuthenticationContext";
import { Form, Button } from "../../Core/Forms/Components";
import { FormField, FormFieldLabel, FormFieldTextInput } from "../../Core/Forms/Components/FormFields";
import { FormFieldGroup } from "../../Core/Forms/Components/FormFields/FormField";
import { useFormFlow } from "../../Core/PageFlows";
import { Content, Header } from "../../Core/PageParts";
import { LoginFormHandler } from "./FormHandlers/LoginFormHandler";
import { LoginForm } from "./Forms/LoginForm";

export const LoginRoute: NonIndexRouteObject = {
    path: "/login",
    Component: LoginPage
};

function LoginPage(): React.JSX.Element {
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
            <Header>
                HintKeep - Login
            </Header>

            <Content>
                <p>
                    Welcome to HintKeep, please provide your credentials to start using the app!
                </p>

                <Form
                    isLoading={isLoggingIn}
                    onSubmit={logInAsync}
                >
                    <FormFieldGroup>
                        <FormField field={form.username}>
                            <FormFieldLabel />
                            <FormFieldTextInput placeholder="name1983" />
                        </FormField>

                        <FormField field={form.password}>
                            <FormFieldLabel />
                            <FormFieldTextInput
                                type="password"
                                placeholder="admin"
                            />
                        </FormField>
                    </FormFieldGroup>

                    <div className="toolbar">
                        <Button
                            type="submit"
                            text="Login"
                            processing={isLoggingIn}
                        />
                        <Link to="/sign-up">
                            Sign up
                        </Link>

                        <Link to="/">
                            Cancel
                        </Link>
                    </div>
                </Form>
            </Content>
        </>
    );
}