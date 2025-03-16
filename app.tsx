import React from "react";
import { BrowserRouter, Route, Routes } from "react-router";
import { HomePage } from "./Pages/Home";
import { IndexedDatabaseScoped } from "./Pages/IndexedDatabaseScoped";
import { Layout } from "./Pages/Layout";
import { LoginPage } from "./Pages/Login";

import "./app.scss";

export function App(): React.JSX.Element {
    return (
        <BrowserRouter>
            <Routes>
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
                    </Route>
                </Route>
            </Routes>
        </BrowserRouter>
    );
}