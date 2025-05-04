import React, { useEffect } from "react";
import { Link, useNavigate } from "react-router";
import { Form, SubmitButton } from "../../Core/Forms/Components";
import { FormField, FormFieldCheckbox, FormFieldLabel, FormFieldTextInput } from "../../Core/Forms/Components/FormFields";
import { useFormFlow } from "../../Core/PageFlows";
import { AccountFormHandler } from "./FormHandlers/AccountFormHandler";
import { AccountForm } from "./Forms/AccountForm";

export function AccountAddPage(): React.JSX.Element {
    const navigate = useNavigate();

    const formFlow = useFormFlow({
        form: AccountForm,
        formHandler: AccountFormHandler,

        confirmationPrompt: {
            message: "Any unsaved changes will be discarded, continue?",
            confirmButtonLabel: "Yes, cancel",
            dismissButtonLabel: "No, continue adding hint"
        }
    });
    const { form, isSubmitted } = formFlow;

    useEffect(
        () => {
            if (isSubmitted)
                navigate("/");
        },
        [isSubmitted, navigate]
    );

    return (
        <>
            <h1>
                Add hint
            </h1>
            <Form pageFlow={formFlow}>
                <FormField field={form.name}>
                    <FormFieldLabel />
                    <FormFieldTextInput />
                </FormField>

                <FormField field={form.username}>
                    <FormFieldLabel />
                    <FormFieldTextInput />
                </FormField>

                <FormField field={form.hint}>
                    <FormFieldLabel />
                    <FormFieldTextInput />
                </FormField>

                <FormField field={form.pinned}>
                    <FormFieldLabel />
                    <FormFieldCheckbox />
                </FormField>

                <FormField field={form.notes}>
                    <FormFieldLabel />
                    <FormFieldTextInput multiline />
                </FormField>

                <div>
                    <SubmitButton text="Save" />
                    <Link to="/">
                        Cancel
                    </Link>
                </div>
            </Form>
        </>
    );
}