import React, { useMemo } from "react";
import { useViewModelDependency } from "react-model-view-viewmodel";
import { createBrowserRouter, Navigate, Outlet, useMatch } from "react-router";
import { CurrentUserProvider } from "../Core/Authentication";
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
                        const { user } = useViewModelDependency(CurrentUserProvider);
                        const match = useMatch({
                            path: "/",
                            end: true
                        });

                        if (match && user === null)
                            return <LoginPage />;

                        return <Outlet />;
                    },
                    children: [
                        SignUpRoute
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