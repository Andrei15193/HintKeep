import React, { useEffect, useMemo } from "react";
import { generatePath, Link, type NonIndexRouteObject, useParams } from "react-router";
import { LoadingContent } from "../../Core/Loader";
import { useDataSourceFlow } from "../../Core/PageFlows";
import { Breadcrumbs, Content, Header } from "../../Core/PageParts";
import { AccountHintsDataSource, type IAccountScoped } from "./DataSources/AccountHintsDataSource";

export const ActiveAccountHintsRoute: NonIndexRouteObject = {
    path: "/:accountId/hints",
    Component: AccountHints
};

export function AccountHints(): React.JSX.Element {
    const { accountId } = useParams<{ readonly accountId: string }>();

    const options = useMemo<IAccountScoped>(() => ({ accountId: accountId! }), [accountId]);

    const {
        isLoading,
        result: accountHints,
        loadAsync
    } = useDataSourceFlow({
        options,
        dataSource: AccountHintsDataSource
    });

    useEffect(
        () => {
            loadAsync();
        },
        [loadAsync]
    );

    return (
        <>
            <Header>
                HintKeep - View Account History
            </Header>

            <Breadcrumbs
                items={
                    [
                        <Link to="/">
                            Accounts
                        </Link>,
                        accountHints?.account.name && (
                            <Link to={generatePath("/:id", { id: accountHints.account.id })}>
                                {`${accountHints?.account.name} Account`}
                            </Link>
                        ),
                        "History"
                    ]
                }
            >
                <Link to={generatePath("/:id", { id: accountId! })}>
                    Back
                </Link>
            </Breadcrumbs>

            <Content>
                <LoadingContent isLoading={isLoading}>
                    <table className="account-hints">
                        <thead>
                            <tr>
                                <th>
                                    Hint
                                </th>
                                <th>
                                    Date Added
                                </th>
                            </tr>
                        </thead>
                        <tbody>
                            {
                                accountHints && accountHints.items.map((accountHint) => (
                                    <tr key={accountHint.id}>
                                        <td>
                                            {accountHint.hint}
                                        </td>
                                        <td>
                                            {accountHint.dateAdded.toLocaleDateString("en-GB", { dateStyle: "medium" })}
                                        </td>
                                    </tr>
                                ))
                            }
                        </tbody>
                    </table>
                </LoadingContent>
            </Content>
        </>
    );
}