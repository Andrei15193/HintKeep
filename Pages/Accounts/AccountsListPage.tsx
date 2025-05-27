import type { IAccountListItem } from "./Models/IAccountListItem";
import React, { useCallback, useRef } from "react";
import { useViewModelMemo } from "react-model-view-viewmodel";
import { generatePath, Link } from "react-router";
import { useAuthenticatedUser } from "../../Core/Contexts/AuthenticationContext";
import { Form, Button } from "../../Core/Forms/Components";
import { FormField, FormFieldTextInput } from "../../Core/Forms/Components/FormFields";
import { HintKeepFormField } from "../../Core/Forms/ViewModels";
import { useDataSourceFlow } from "../../Core/PageFlows";
import { Content, Header } from "../../Core/PageParts";
import { type ISearchText, type IListResult, AccountsDataSource } from "./DataSources/AccountsDataSource";

export function AccountsListPage(): React.JSX.Element {
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
        (): ISearchText => ({
            searchText: searchTextField.value
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
                {`HintKeep - ${username} Accounts`}
            </Header>

            <nav>
                <Link to="/">
                    Accounts
                </Link>
                <Link to="/profile">
                    Profile
                </Link>
            </nav>

            <Content>
                {
                    totalCount === 0
                        ? (
                            <>
                                {
                                    isLoading
                                        ? "Loading"
                                        : (
                                            <p>
                                                Sadly, you do not like storing hints... Maybe we can change that!
                                            </p>
                                        )
                                }
                                <Link to="/add">
                                    Add account
                                </Link>
                            </>
                        )
                        : (
                            <>
                                <Form
                                    isLoading={false}
                                    onSubmit={loadAsync}
                                    className="toolbar"
                                >
                                    <Link to="add">
                                        Add account
                                    </Link>
                                    <FormField field={searchTextField}>
                                        <FormFieldTextInput type="search" />
                                    </FormField>
                                    <Button
                                        type="submit"
                                        text="Search"
                                    />
                                </Form>

                                {
                                    isLoading
                                        ? "Loading"
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
                                                                        <Link to={generatePath("/:id", { id: account.id })}>
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
                            </>
                        )
                }
            </Content>
        </>
    );
}