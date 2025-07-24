import React from "react";
import { createBrowserRouter, Navigate, Outlet, useMatch } from "react-router";
import { useAuthentication } from "../Core/Contexts/AuthenticationContext";
import { ActiveAccountHintsRoute } from "./AccountHints";
import { AccountAddRoute, ActiveAccountDetailsRoute, ActiveAccountsRoute, ArchivedAccountDetailsRoute, ArchivedAccountsRoute } from "./Accounts";
import { HomePage } from "./Home";
import { IndexedDatabaseScoped } from "./IndexedDatabaseScoped";
import { LoginRoute } from "./Login";
import { SignUpRoute } from "./SignUp";
import { UserProfileRoute } from "./User";

export const AppRouter = createBrowserRouter([
    {
        path: "/",
        Component() {
            const { user } = useAuthentication();
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
                            const { user } = useAuthentication();

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
]);