import React, { useEffect, useMemo } from "react";
import { useViewModel } from "react-model-view-viewmodel";
import { type NonIndexRouteObject, Link, useNavigate, useParams } from "react-router";
import { blankSubmit, Button, Form } from "../../../../Core/Forms/Components";
import { FormField, FormFieldCheckbox, FormFieldLabel, FormFieldTextInput } from "../../../../Core/Forms/Components/FormFields";
import { useDataSourceFlow, useFormFlow } from "../../../../Core/PageFlows";
import { Breadcrumbs, Header } from "../../../../Core/PageParts";
import { AccountDetailsDataSource } from "../../DataSources/AccountDetailsDataSource";
import { AccountDeletionFormHandler } from "../../FormHandlers/AccountDeletionFormHandler";
import { AccountForm } from "../../Forms/AccountForm";

export const ArchivedAccountDetailsRoute: NonIndexRouteObject = {
    path: "/archived/:id",
    Component: ArchivedAccountDetailsPageView
};

function ArchivedAccountDetailsPageView(): React.JSX.Element {
    const { id } = useParams<{ readonly id: string }>();
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
                navigate("/archived");
        },
        [isDeleted, navigate]
    );

    return (
        <>
            <Header>
                HintKeep - View Archived Account
            </Header>

            <Breadcrumbs items={["Archived Accounts", account?.name && `${account?.name} Account`]}>
                <Link to="/archived">
                    Back
                </Link>
            </Breadcrumbs>

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
                        text="Delete"
                        onClick={deleteAsync}
                        danger
                    />
                </div>
            </Form>
        </>
    );
}