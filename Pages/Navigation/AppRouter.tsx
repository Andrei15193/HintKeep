import React, { useEffect } from "react";
import { createBrowserRouter, Outlet, useMatch, useNavigate } from "react-router";
import { AccountAddPage, AccountsListPage } from "../Accounts";
import { useUser } from "../Contexts/UserContext";
import { HomePage } from "../Home";
import { IndexedDatabaseScoped } from "../IndexedDatabaseScoped";
import { Layout } from "../Layout";
import { LoginPage } from "../Login";
import { SignUpPage } from "../SignUp/SignUpPage";

export const AppRouter = createBrowserRouter([
    {
        path: "/",
        Component() {
            const user = useUser();
            const match = useMatch({
                path: "/",
                end: true
            });

            if (match)
                if (user === null)
                    return <HomePage />;
                else
                    return (
                        <IndexedDatabaseScoped>
                            <AccountsListPage />
                        </IndexedDatabaseScoped>
                    );
            else
                return <Outlet />;
        },
        children: [
            {
                Component: IndexedDatabaseScoped,
                children: [
                    {
                        Component: Layout,
                        children: [
                            {
                                Component() {
                                    const user = useUser();
                                    const navigate = useNavigate();

                                    useEffect(
                                        () => {
                                            if (user !== null)
                                                navigate("/");
                                        },
                                        [user, navigate]
                                    );

                                    if (user === null)
                                        return <Outlet />;

                                    return null;
                                },
                                children: [
                                    {
                                        path: "login",
                                        Component: LoginPage
                                    },
                                    {
                                        path: "sign-up",
                                        Component: SignUpPage
                                    }
                                ]
                            },
                            {
                                Component() {
                                    const user = useUser();
                                    const navigate = useNavigate();

                                    useEffect(
                                        () => {
                                            if (user === null)
                                                navigate("/");
                                        },
                                        [user, navigate]
                                    );

                                    if (user !== null)
                                        return <Outlet />;

                                    return null;
                                },
                                children: [
                                    {
                                        path: "add",
                                        Component: AccountAddPage
                                    }
                                ]
                            }
                        ]
                    }
                ]
            }
        ]
    }
]);