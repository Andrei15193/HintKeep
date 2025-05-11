import React, { useEffect } from "react";
import { Link } from "react-router";
import { useAuthentication } from "../../Core/Contexts/AuthenticationContext";
import { Form, SubmitButton } from "../../Core/Forms/Components";
import { FormField, FormFieldError, FormFieldLabel, FormFieldTextInput } from "../../Core/Forms/Components/FormFields";
import { useFormFlow } from "../../Core/PageFlows";
import { usePromptedNavigate } from "../../Core/Prompt";
import { SignUpFormHandler } from "./FormHandlers/SignUpFormHandler";
import { SignUpForm } from "./Forms/SignUpForm";

export function SignUpPage(): React.JSX.Element {
    const { authenticate } = useAuthentication();
    const {
        form,
        result: user,
        isSubmitting: isSigningUp,
        isSubmitted: isSignedUp,
        submitAsync: signUpAsync
    } = useFormFlow({
        form: SignUpForm,
        formHandler: SignUpFormHandler
    });

    const navigate = usePromptedNavigate({ blockNavigation: !isSignedUp });

    useEffect(
        () => {
            if (isSignedUp) {
                const navigationResult = navigate("/");
                if (navigationResult === true)
                    authenticate(user);
                else if (typeof navigationResult === "object")
                    navigationResult.then((result) => {
                        if (result)
                            authenticate(user);
                    });
            }
        },
        [isSignedUp, user, authenticate, navigate]
    );

    return (
        <>
            <h2>
                Sign Up
            </h2>

            <p>
                Welcome to HintKeep, please provide the following information to start using the app!
            </p>

            <Form
                isLoading={isSigningUp}
                onSubmit={signUpAsync}
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

                <FormField field={form.hint}>
                    <FormFieldLabel />
                    <FormFieldTextInput multiline />
                    <FormFieldError />
                </FormField>

                <SubmitButton text="Sign Up" />

                <Link to="/login">
                    Cancel
                </Link>
            </Form>
        </>
    );
}