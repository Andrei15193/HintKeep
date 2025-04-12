import React from "react";
import { BrowserRouter, Navigate, Route, Routes } from "react-router";
import { AccountAddPage, AccountsListPage } from "../Accounts";
import { useUser } from "../Contexts/UserContext";
import { HomePage } from "../Home";
import { IndexedDatabaseScoped } from "../IndexedDatabaseScoped";
import { Layout } from "../Layout";
import { LoginPage } from "../Login";
import { SignUpPage } from "../SignUp/SignUpPage";

export function AppRouter(): React.JSX.Element {
    const user = useUser();

    return (
        <BrowserRouter>
            <Routes>
                {
                    user === null
                        ? (
                            <Route Component={Layout}>
                                <Route
                                    index
                                    Component={HomePage}
                                />
                                <Route Component={IndexedDatabaseScoped}>
                                    <Route
                                        path="login"
                                        Component={LoginPage}
                                    />
                                    <Route
                                        path="sign-up"
                                        Component={SignUpPage}
                                    />
                                </Route>
                            </Route>
                        )
                        : (
                            <Route Component={IndexedDatabaseScoped}>
                                <Route Component={Layout}>
                                    <Route
                                        index
                                        Component={AccountsListPage}
                                    />
                                    <Route
                                        path="add"
                                        Component={AccountAddPage}
                                    />
                                </Route>
                            </Route>
                        )
                }
                <Route
                    path="*"
                    element={
                        <Navigate
                            to="/"
                            replace
                        />
                    }
                />
            </Routes>
        </BrowserRouter>
    );
}