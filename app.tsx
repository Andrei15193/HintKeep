import React from "react";
import { BrowserRouter, Route, Routes } from "react-router";
import { Layout } from "./Pages/Layout";
import { Login } from "./Pages/Login/Login";

import "./app.scss";

export function App(): React.JSX.Element {
    return (
        <BrowserRouter>
            <Routes>
                <Route Component={Layout}>
                    <Route
                        index
                        Component={Login}
                    />
                </Route>
            </Routes>
        </BrowserRouter>
    );
}