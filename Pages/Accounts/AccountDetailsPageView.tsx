import React from "react";
import { Link, useParams } from "react-router";
import { Form } from "../../Core/Forms/Components";
import { FormField, FormFieldCheckbox, FormFieldLabel, FormFieldTextInput } from "../../Core/Forms/Components/FormFields";
import { useEditFlow } from "../../Core/PageFlows";
import { useViewEditToggleContext } from "../../Core/ViewEditToggle";
import { AccountDetailsDataSource } from "./DataSources/AccountDetailsDataSource";
import { AccountForm } from "./Forms/AccountForm";

export function AccountDetailsPageView(): React.JSX.Element {
    const { id } = useParams<{ readonly id: string }>();
    const { goToEditMode } = useViewEditToggleContext();

    const formFlow = useEditFlow({
        entityId: id!,
        dataSource: AccountDetailsDataSource,
        form: AccountForm,
        formHandler: null!,
        skipConfirmationPrompt: true
    });
    const { form } = formFlow;

    return (
        <>
            <h1>
                View Account
            </h1>
            <Form
                pageFlow={formFlow}
                disabled
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
                    <button
                        type="button"
                        onClick={goToEditMode}
                    >
                        Edit
                    </button>
                    <Link to="/">
                        Cancel
                    </Link>
                </div>
            </Form>
        </>
    );
}