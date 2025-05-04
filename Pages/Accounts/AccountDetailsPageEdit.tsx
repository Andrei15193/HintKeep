import React, { useEffect } from "react";
import { generatePath, Link, useParams } from "react-router";
import { Form, SubmitButton } from "../../Core/Forms/Components";
import { FormField, FormFieldCheckbox, FormFieldLabel, FormFieldTextInput } from "../../Core/Forms/Components/FormFields";
import { useEditFlow } from "../../Core/PageFlows";
import { useViewEditToggleContext } from "../../Core/ViewEditToggle";
import { useShowConfirmationPrompt } from "../Prompt";
import { AccountDetailsDataSource } from "./DataSources/AccountDetailsDataSource";
import { AccountFormHandler } from "./FormHandlers/AccountFormHandler";
import { AccountForm } from "./Forms/AccountForm";

export function AccountDetailsPageEdit(): React.JSX.Element {
    const { id } = useParams<{ readonly id: string }>();
    const { goToViewMode } = useViewEditToggleContext();

    const formFlow = useEditFlow({
        entityId: id!,
        dataSource: AccountDetailsDataSource,
        form: AccountForm,
        formHandler: AccountFormHandler
    });
    const { form, isSubmitted } = formFlow;

    const discardChangesCallback = useShowConfirmationPrompt({
        onConfirm: goToViewMode
    });

    useEffect(
        () => {
            if (isSubmitted)
                goToViewMode();
        },
        [isSubmitted, goToViewMode]
    );

    return (
        <>
            <h1>
                Edit Account
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
                    <Link
                        to={generatePath("/:id", { id: id || "" })}
                        onClick={discardChangesCallback}
                        replace
                    >
                        Cancel
                    </Link>
                </div>
            </Form>
        </>
    );
}