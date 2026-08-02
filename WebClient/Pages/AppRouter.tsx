import React, { useMemo } from "react";
import { useViewModelDependency, DependencyResolverScope } from "react-model-view-viewmodel";
import { createBrowserRouter, Navigate, Outlet, useMatch } from "react-router";
import { CurrentUserProvider } from "../Core/Authentication";
import { StorageContext } from "../Core/Data";
import { LoginPage } from "./Login";
import { SignUpRoute } from "./SignUp";
import { useWindow } from "./WindowContext";

export function useAppRouter(): ReturnType<typeof createBrowserRouter> {
    const window = useWindow();

    return useMemo(
        () => createBrowserRouter(
            [
                {
                    path: "/",
                    Component() {
                        const { storageType } = useViewModelDependency(StorageContext);
                        const { user } = useViewModelDependency(CurrentUserProvider);
                        const isRootMatch = useMatch({
                            path: "/",
                            end: true
                        });

                        if (isRootMatch && user === null)
                            return <LoginPage />;

                        return (
                            <DependencyResolverScope deps={[storageType, user]}>
                                <Outlet />
                            </DependencyResolverScope>
                        );
                    },
                    children: [
                        {
                            path: "/",
                            Component() {
                                return (
                                    <div>
                                        HintKeep - Accounts
                                    </div>
                                );
                            }
                        }
                        // {
                        //     Component() {
                        //         const { user } = useViewModelDependency(CurrentUserProvider);

                        //         if (user === null)
                        //             return <Navigate to="/" />;

                        //         return <Outlet />;
                        //     },
                        //     children: [
                        //         ActiveAccountsRoute,
                        //         UserProfileRoute,
                        //         ArchivedAccountsRoute,
                        //         ArchivedAccountDetailsRoute,
                        //         AccountAddRoute,
                        //         ActiveAccountDetailsRoute,
                        //         ActiveAccountHintsRoute
                        //     ]
                        // }
                    ]
                },
                SignUpRoute,
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