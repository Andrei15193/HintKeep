import React, { useEffect, useMemo } from "react";
import { useViewModel } from "react-model-view-viewmodel";
import { Link, useNavigate, useParams } from "react-router";
import { blankSubmit, Button, Form } from "../../Core/Forms/Components";
import { FormField, FormFieldCheckbox, FormFieldLabel, FormFieldTextInput } from "../../Core/Forms/Components/FormFields";
import { useDataSourceFlow, useFormFlow } from "../../Core/PageFlows";
import { Header } from "../../Core/PageParts";
import { useViewEditToggleContext } from "../../Core/ViewEditToggle";
import { AccountDetailsDataSource } from "./DataSources/AccountDetailsDataSource";
import { AccountDeletionFormHandler } from "./FormHandlers/AccountDeletionFormHandler";
import { AccountForm } from "./Forms/AccountForm";

export function AccountDetailsPageView(): React.JSX.Element {
    const { id } = useParams<{ readonly id: string }>();
    const { goToEditMode } = useViewEditToggleContext();
    const navigate = useNavigate();

    const dataSourceOptions = useMemo(() => ({ id }), [id]);
    const { isLoading, result: account } = useDataSourceFlow({
        options: dataSourceOptions,
        dataSource: AccountDetailsDataSource
    });

    const form = useViewModel(AccountForm, [account]);
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

    useEffect(
        () => {
            if (isDeleted)
                navigate("/");
        },
        [isDeleted, navigate]
    );

    return (
        <>
            <Header>
                {`HintKeep - View ${form.name.value} Account`}
            </Header>

            <Form
                isLoading={isLoading || isDeleting}
                onSubmit={blankSubmit}
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
                        text="Edit"
                        onClick={goToEditMode}
                    />
                    <Button
                        text="Delete"
                        onClick={deleteAsync}
                        danger
                    />
                    <Link to="/">
                        Cancel
                    </Link>
                </div>
            </Form>
        </>
    );
}