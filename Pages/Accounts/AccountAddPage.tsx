import React, { useEffect } from "react";
import { Link } from "react-router";
import { Form, Button } from "../../Core/Forms/Components";
import { FormField, FormFieldCheckbox, FormFieldLabel, FormFieldTextInput } from "../../Core/Forms/Components/FormFields";
import { useFormFlow } from "../../Core/PageFlows";
import { Header } from "../../Core/PageParts";
import { usePromptedNavigate } from "../../Core/Prompt";
import { AccountFormHandler } from "./FormHandlers/AccountFormHandler";
import { AccountForm } from "./Forms/AccountForm";

export function AccountAddPage(): React.JSX.Element {
    const {
        form,
        isSubmitting,
        isSubmitted,
        submitAsync
    } = useFormFlow({
        form: AccountForm,
        formHandler: AccountFormHandler
    });

    const navigate = usePromptedNavigate({
        blockNavigation: !isSubmitted,
        message: "Any unsaved changes will be discarded, continue?",
        confirmButtonLabel: "Yes, cancel",
        dismissButtonLabel: "No, continue adding hint"
    });

    useEffect(
        () => {
            if (isSubmitted)
                navigate("/");
        },
        [isSubmitted, navigate]
    );

    return (
        <>
            <Header>
                HintKeep - Add Account
            </Header>

            <Form
                isLoading={isSubmitting}
                onSubmit={submitAsync}
            >
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
                    <FormFieldTextInput multiline />
                </FormField>

                <FormField field={form.pinned}>
                    <FormFieldLabel />
                    <FormFieldCheckbox />
                </FormField>

                <FormField field={form.notes}>
                    <FormFieldLabel />
                    <FormFieldTextInput multiline />
                </FormField>

                <div className="toolbar">
                    <Button
                        type="submit"
                        text="Save"
                        processing={isSubmitting}
                    />
                    <Link to="/">
                        Cancel
                    </Link>
                </div>
            </Form>

        </>
    );
}