import React from "react";
import { BrowserRouter, Route, Routes } from "react-router";
import { IndexedDatabaseProvider } from "./Data/IndexedDatabase";
import { HintKeepDatabaseDefinition } from "./Data/IndexedDatabase/HintKeep/HintKeepDatabaseDefinition";
import { HintKeepDependencyContainerProvider } from "./Dependencies";
import { UserContextProvider, useUser } from "./Pages/Contexts/UserContext";
import { HintsListPage } from "./Pages/HintsList";
import { HomePage } from "./Pages/Home";
import { IndexedDatabaseScoped } from "./Pages/IndexedDatabaseScoped";
import { Layout } from "./Pages/Layout";
import { LoginPage } from "./Pages/Login";
import { SignUpPage } from "./Pages/SignUp/SignUpPage";

import "./App.scss";

export function App(): React.JSX.Element {
    return (
        <UserContextProvider>
            <IndexedDatabaseProvider databaseDefinition={HintKeepDatabaseDefinition}>
                <HintKeepDependencyContainerProvider>
                    <AppRoutes />
                </HintKeepDependencyContainerProvider>
            </IndexedDatabaseProvider>
        </UserContextProvider>
    );
}

export function AppRoutes(): React.JSX.Element {
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
                                        Component={HintsListPage}
                                    />
                                </Route>
                            </Route>
                        )
                }
            </Routes>
        </BrowserRouter>
    );
}