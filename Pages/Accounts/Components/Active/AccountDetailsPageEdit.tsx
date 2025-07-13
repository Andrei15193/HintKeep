import React, { useEffect, useMemo } from "react";
import { useViewModel } from "react-model-view-viewmodel";
import { generatePath, Link, useParams } from "react-router";
import { Form, Button } from "../../../../Core/Forms/Components";
import { FormField, FormFieldCheckbox, FormFieldLabel, FormFieldTextInput } from "../../../../Core/Forms/Components/FormFields";
import { useDataSourceFlow, useFormFlow } from "../../../../Core/PageFlows";
import { Header } from "../../../../Core/PageParts";
import { useShowConfirmationPrompt, usePromptedNavigate } from "../../../../Core/Prompt";
import { useViewEditToggleContext } from "../../../../Core/ViewEditToggle";
import { AccountDetailsDataSource } from "../../DataSources/AccountDetailsDataSource";
import { AccountArchivalFormHandler } from "../../FormHandlers/AccountArchivalFormHandler";
import { AccountDeletionFormHandler } from "../../FormHandlers/AccountDeletionFormHandler";
import { AccountFormHandler } from "../../FormHandlers/AccountFormHandler";
import { AccountForm } from "../../Forms/AccountForm";

export function AccountDetailsPageEdit(): React.JSX.Element {
    const { id } = useParams<{ readonly id: string }>();
    const { goToViewMode } = useViewEditToggleContext();

    const dataSourceOptions = useMemo(() => ({ id }), [id]);
    const { isLoading, result: account } = useDataSourceFlow({
        options: dataSourceOptions,
        dataSource: AccountDetailsDataSource
    });

    const form = useViewModel(AccountForm, [account]);
    const {
        isSubmitting: isSaving,
        isSubmitted: isSaved,
        submitAsync: saveAsync
    } = useFormFlow({
        form,
        formHandler: AccountFormHandler
    });
    const {
        isSubmitting: isArchiving,
        isSubmitted: isArchived,
        submitAsync: archiveAsync
    } = useFormFlow({
        form,
        formHandler: AccountArchivalFormHandler,

        notifications: {
            successMessage: `Account '${account?.name}' has been archived.`
        },

        confirmationPrompt: {
            message: "Archived accounts are still available, but not easily accessible, do you wish to continue?",
            confirmButtonLabel: "Yes, archive the account",
            dismissButtonLabel: "No, I do not want to archive the account"
        }
    });
    const {
        isSubmitting: isDeleting,
        isSubmitted: isDeleted,
        submitAsync: deleteAsync
    } = useFormFlow({
        form,
        formHandler: AccountDeletionFormHandler,

        notifications: {
            successMessage: `Account '${account?.name}' has been deleted.`
        },

        confirmationPrompt: {
            message: "This action cannot be reversed, are you sure?",
            confirmButtonLabel: "Yes, permanently delete the account",
            dismissButtonLabel: "No, I do not want to delete the account"
        }
    });
    const navigate = usePromptedNavigate({
        blockNavigation: !isSaved && !isArchived && !isDeleted
    });

    const discardChangesCallback = useShowConfirmationPrompt({
        onConfirm: goToViewMode
    });

    useEffect(
        () => {
            if (isSaved)
                goToViewMode();
            if (isArchived || isDeleted)
                navigate("/");
        },
        [isSaved, isArchived, isDeleted, goToViewMode, navigate]
    );

    return (
        <>
            <Header>
                {`HintKeep - Edit ${form.name.value} Account`}
            </Header>

            <Form
                isLoading={isLoading || isSaving || isArchiving || isDeleting}
                onSubmit={saveAsync}
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
                        processing={isSaving}
                        disabled={isLoading || isSaving || isDeleting}
                    />
                    <Button
                        text="Archive"
                        onClick={archiveAsync}
                        neutral
                    />
                    <Button
                        text="Delete"
                        danger
                        processing={isDeleting}
                        disabled={isLoading || isSaving || isDeleting}
                        onClick={deleteAsync}
                    />
                    <Link
                        replace
                        to={generatePath("/:id", { id: id || "" })}
                        className="danger"
                        onClick={discardChangesCallback}
                    >
                        Cancel
                    </Link>
                </div>
            </Form>
        </>
    );
}