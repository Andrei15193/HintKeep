import React, { useLayoutEffect } from "react";
import { createBrowserRouter, Outlet, useMatch, useNavigate } from "react-router";
import { AccountAddPage, AccountDetailsPage, AccountsListPage } from "../Accounts";
import { useUser } from "../Contexts/UserContext";
import { HomePage } from "../Home";
import { IndexedDatabaseScoped } from "../IndexedDatabaseScoped";
import { Layout } from "../Layout";
import { LoginPage } from "../Login";
import { SignUpPage } from "../SignUp/SignUpPage";
import { UserProfilePage } from "../User";

export const AppRouter = createBrowserRouter([
    {
        Component: Layout,
        children: [
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
                                Component() {
                                    const user = useUser();
                                    const navigate = useNavigate();

                                    useLayoutEffect(
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

                                    useLayoutEffect(
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
                                        path: "profile",
                                        Component: UserProfilePage
                                    },
                                    {
                                        path: "add",
                                        Component: AccountAddPage
                                    },
                                    {
                                        path: ":id",
                                        Component: AccountDetailsPage
                                    }
                                ]
                            }
                        ]
                    }
                ]
            }
        ]
    },
    {
        path: "*",
        Component() {
            const navigate = useNavigate();

            useLayoutEffect(
                () => {
                    navigate("/");
                },
                [navigate]
            );

            return null;
        }
    }
]);