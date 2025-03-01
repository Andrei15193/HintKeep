import { BrowserRouter, Route, Routes } from "react-router";
import { Layout } from "./Pages/Layout";
import { Login } from "./Pages/Login/Login";

import "./app.scss";

Promise
    .all([
        import("react"),
        import("react-dom/client")
    ])
    .then(([{ createElement }, { createRoot }]) => {
        createRoot(document.body.appendChild(document.createElement("div")))
            .render(
                createElement(BrowserRouter, {
                    children: createElement(Routes, {
                        children: createElement(Route, {
                            Component: Layout,
                            children: createElement(Route, {
                                index: true,
                                Component: Login
                            })
                        })
                    })
                })
            );
    });