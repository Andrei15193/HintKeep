import React, { useMemo } from "react";
import { useViewModelDependency } from "react-model-view-viewmodel";
import { createBrowserRouter, Navigate, Outlet, useMatch } from "react-router";
import { CurrentUserProvider } from "../Core/Authentication";
import { ActiveAccountHintsRoute } from "./AccountHints";
import { AccountAddRoute, ActiveAccountDetailsRoute, ActiveAccountsRoute, ArchivedAccountDetailsRoute, ArchivedAccountsRoute } from "./Accounts";
import { HomePage } from "./Home";
import { IndexedDatabaseScoped } from "./IndexedDatabaseScoped";
import { LoginRoute } from "./Login";
import { SignUpRoute } from "./SignUp";
import { UserProfileRoute } from "./User";
import { useWindow } from "./WindowContext";

export function useAppRouter(): ReturnType<typeof createBrowserRouter> {
    const window = useWindow();

    return useMemo(
        () => createBrowserRouter(
            [
                {
                    path: "/",
                    Component() {
                        const { user } = useViewModelDependency(CurrentUserProvider);
                        const match = useMatch({
                            path: "/",
                            end: true
                        });

                        if (match && user === null)
                            return <HomePage />;

                        return <Outlet />;
                    },
                    children: [
                        {
                            Component: IndexedDatabaseScoped,
                            children: [
                                LoginRoute,
                                SignUpRoute,
                                {
                                    Component() {
                                        const { user } = useViewModelDependency(CurrentUserProvider);

                                        if (user === null)
                                            return <Navigate to="/" />;

                                        return <Outlet />;
                                    },
                                    children: [
                                        ActiveAccountsRoute,
                                        UserProfileRoute,
                                        ArchivedAccountsRoute,
                                        ArchivedAccountDetailsRoute,
                                        AccountAddRoute,
                                        ActiveAccountDetailsRoute,
                                        ActiveAccountHintsRoute
                                    ]
                                }
                            ]
                        }
                    ]
                },
                {
                    path: "*",
                    element: <Navigate to="/" />
                }
            ],
            {
                window
            }
        ),
        [window]
    );
}