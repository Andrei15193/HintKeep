import type { IAccountListItem } from "./Models/IAccountListItem";
import React, { type FormEvent, useCallback, useEffect, useRef, useState } from "react";
import { useDependency, useViewModelMemo } from "react-model-view-viewmodel";
import { generatePath, Link } from "react-router";
import { HintKeepFormField } from "../../Forms";
import { useUser } from "../Contexts/UserContext";
import { TextInput } from "../Forms";
import { AccountsDataSource } from "./DataSources/AccountsDataSource";

export function AccountsListPage(): React.JSX.Element {
    const { username } = useUser()!;
    const accountsDataSource = useDependency(AccountsDataSource);
    const searchTextInputRef = useRef<HTMLInputElement | null>(null);
    const searchTextField = useViewModelMemo(
        () => new HintKeepFormField<string>({
            name: "search",
            label: "Search",
            initialValue: ""
        }),
        []
    );

    const [isLoading, setIsLoading] = useState(true);
    const accountsTotalCountRef = useRef(0);
    const [accounts, setAccounts] = useState<readonly IAccountListItem[]>([]);

    const loadAccountsAsyncCallback = useCallback(
        async (searchText: string) => {
            setIsLoading(true);
            try {
                const { items: accounts, totalCount } = await accountsDataSource.getDataAsync({ searchText });
                accountsTotalCountRef.current = totalCount;
                setAccounts(accounts);
            }
            finally {
                setIsLoading(false);
            }
        },
        [accountsDataSource]
    );

    const searchAccountsCallback = useCallback(
        (event: FormEvent<HTMLFormElement>) => {
            event.preventDefault();
            loadAccountsAsyncCallback(searchTextField.value);
        },
        [searchTextField, loadAccountsAsyncCallback]
    );

    useEffect(
        () => {
            loadAccountsAsyncCallback("");
        },
        [accountsDataSource, loadAccountsAsyncCallback]
    );

    return (
        <>
            <h1>
                Hello
                {" "}
                {username}
                !
            </h1>
            {
                accountsTotalCountRef.current === 0
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
                            <Link to="add">
                                Add account
                            </Link>
                        </>
                    )
                    : (
                        <>
                            <Link to="add">
                                Add account
                            </Link>
                            <form onSubmit={searchAccountsCallback}>
                                <TextInput
                                    ref={searchTextInputRef}
                                    field={searchTextField}
                                />
                                <button
                                    type="submit"
                                    hidden
                                >
                                    Search
                                </button>
                            </form>

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
        </>
    );
}