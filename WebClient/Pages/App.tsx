import React from "react";
import { RouterProvider } from "react-router";
import { ModalsDisplay } from "../Core/Modals";
import { GlobalNotificationsContainer } from "../Core/Notifications";
import { useAppRouter } from "./AppRouter";
import { ConfirmationPrompt } from "./ConfirmationPrompt";

export function App(): React.JSX.Element {
    const appRouter = useAppRouter();

    return (
        <>
            <RouterProvider router={appRouter} />

            <ModalsDisplay />
            <ConfirmationPrompt />
            <aside className="global-notifications">
                <GlobalNotificationsContainer />
            </aside>
        </>
    );
}