import React, { useEffect } from "react";
import { Link, useNavigate } from "react-router";
import { useAuthentication } from "../../Core/Contexts/AuthenticationContext";
import { Form, SubmitButton } from "../../Core/Forms/Components";
import { FormField, FormFieldError, FormFieldLabel, FormFieldTextInput } from "../../Core/Forms/Components/FormFields";
import { useFormFlow } from "../../Core/PageFlows";
import { SignUpFormHandler } from "./FormHandlers/SignUpFormHandler";
import { SignUpForm } from "./Forms/SignUpForm";

export function SignUpPage(): React.JSX.Element {
    const navigate = useNavigate();
    const { authenticate } = useAuthentication();
    const formFlow = useFormFlow({
        form: SignUpForm,
        formHandler: SignUpFormHandler
    });
    const {
        form,
        isSubmitted,
        result: user
    } = formFlow;

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

            <Form pageFlow={formFlow}>
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