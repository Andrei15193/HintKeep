import type { IAccountListItem } from "../../Models/IAccountListItem";
import React, { useCallback, useRef } from "react";
import { useViewModelMemo } from "react-model-view-viewmodel";
import { generatePath, Link } from "react-router";
import { useAuthenticatedUser } from "../../../../Core/Contexts/AuthenticationContext";
import { Form, Button } from "../../../../Core/Forms/Components";
import { FormField, FormFieldTextInput } from "../../../../Core/Forms/Components/FormFields";
import { HintKeepFormField } from "../../../../Core/Forms/ViewModels";
import { LoadingContent } from "../../../../Core/Loader";
import { useDataSourceFlow } from "../../../../Core/PageFlows";
import { Content, Header } from "../../../../Core/PageParts";
import { type ISearchText, type IListResult, type IStatus, AccountsDataSource } from "../../DataSources/AccountsDataSource";

export function ArchivedAccountsListPage(): React.JSX.Element {
    const { username } = useAuthenticatedUser();

    const searchTextField = useViewModelMemo(
        () => new HintKeepFormField<string>({
            name: "search",
            label: "Search",
            initialValue: ""
        }),
        []
    );

    const dataSourceFlowOptionsCallback = useCallback(
        (): ISearchText & IStatus => ({
            searchText: searchTextField.value,
            status: "archvied"
        }),
        [searchTextField]
    );
    const {
        isLoading,
        result,
        loadAsync
    } = useDataSourceFlow({
        options: dataSourceFlowOptionsCallback,
        dataSource: AccountsDataSource
    });

    const resultRef = useRef<IListResult<IAccountListItem> | undefined>(undefined);
    if (resultRef.current === undefined)
        resultRef.current = {
            items: [],
            totalCount: 0
        };
    if (result !== undefined)
        resultRef.current = result;

    const {
        items: accounts,
        totalCount
    } = resultRef.current;

    return (
        <>
            <Header>
                {`HintKeep - ${username} Archived Accounts`}
            </Header>

            <nav>
                <Link to="/">
                    Accounts
                </Link>
                <Link to="/archived">
                    Archived Accounts
                </Link>
                <Link to="/profile">
                    Profile
                </Link>
            </nav>

            <hr />

            <Content>
                {
                    totalCount > 0 && (
                        <Form
                            isLoading={false}
                            onSubmit={loadAsync}
                            className="toolbar"
                        >
                            <FormField field={searchTextField}>
                                <FormFieldTextInput type="search" />
                            </FormField>
                            <Button
                                type="submit"
                                text="Search"
                            />
                        </Form>
                    )
                }
                <LoadingContent isLoading={isLoading}>
                    {
                        totalCount === 0
                            ? (
                                <>
                                    <p>
                                        All of your accounts are active, none of them are archived.
                                    </p>
                                </>
                            )
                            : (
                                <table>
                                    <thead>
                                        <tr>
                                            <th>
                                                Account
                                            </th>
                                            <th>
                                                Username
                                            </th>
                                            <th>
                                                Hint
                                            </th>
                                            <th>
                                                Pinned
                                            </th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {
                                            accounts.length === 0
                                                ? <tr>
                                                    <td colSpan={3}>
                                                        No account match the search criteria.
                                                    </td>
                                                </tr>
                                                : accounts.map((account) => (
                                                    <tr key={account.id}>
                                                        <td>
                                                            <Link to={generatePath("/archived/:id", { id: account.id })}>
                                                                {account.name}
                                                            </Link>
                                                        </td>
                                                        <td>
                                                            {account.username}
                                                        </td>
                                                        <td>
                                                            {account.hint}
                                                        </td>
                                                        <td>
                                                            {account.isPinned ? "Yes" : "No"}
                                                        </td>
                                                    </tr>
                                                ))
                                        }
                                    </tbody>
                                </table>
                            )
                    }
                </LoadingContent>
            </Content>
        </>
    );
}