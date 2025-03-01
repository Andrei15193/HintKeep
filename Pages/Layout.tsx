import React from "react";
import { Outlet } from "react-router";

export function Layout(): React.JSX.Element {
    return (
        <>
            <h1>
                HintKeep
            </h1>
            <div>
                <Outlet />
            </div>
        </>
    );
}