import React, { useEffect } from "react";
import { Link } from "react-router";
import { useAuthentication } from "../../Core/Contexts/AuthenticationContext";
import { Form, Button } from "../../Core/Forms/Components";
import { FormField, FormFieldLabel, FormFieldTextInput } from "../../Core/Forms/Components/FormFields";
import { FormFieldGroup } from "../../Core/Forms/Components/FormFields/FormField";
import { useFormFlow } from "../../Core/PageFlows";
import { Content, Header } from "../../Core/PageParts";
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
            <Header>
                HintKeep - Sign Up
            </Header>

            <Content>
                <p>
                    Welcome to HintKeep, please provide the following information to start using the app!
                </p>

                <Form
                    isLoading={isSigningUp}
                    onSubmit={signUpAsync}
                >
                    <FormFieldGroup>
                        <FormField field={form.username}>
                            <FormFieldLabel />
                            <FormFieldTextInput placeholder="hello2077" />
                        </FormField>

                        <FormField field={form.password}>
                            <FormFieldLabel />
                            <FormFieldTextInput
                                type="password"
                                placeholder="super admin"
                            />
                        </FormField>
                    </FormFieldGroup>

                    <FormField field={form.hint}>
                        <FormFieldLabel />
                        <FormFieldTextInput
                            multiline
                            placeholder="sudo"
                        />
                    </FormField>

                    <div className="toolbar">
                        <Button
                            type="submit"
                            text="Sign Up"
                            processing={isSigningUp}
                        />

                        <Link to="/login">
                            Cancel
                        </Link>
                    </div>
                </Form>
            </Content>
        </>
    );
}