import type { DependencyContainer } from "react-model-view-viewmodel";
import React from "react";
import { type createBrowserRouter, RouterProvider } from "react-router";
import { IndexedDatabaseContextProvider } from "../Core/Data/IndexedDatabase";
import { HintKeepDatabaseDefinition } from "../Core/Data/IndexedDatabase/HintKeep";
import { HintKeepDependencyContainerProvider } from "../Core/Dependencies";
import { ModalContextProvider, ModalsDisplay } from "../Core/Modals";
import { GlobalNotificationsContainer } from "../Core/Notifications";
import { ConfirmationPromptProvider } from "../Core/Prompt";
import { useAppRouter } from "./AppRouter";
import { ConfirmationPrompt } from "./ConfirmationPrompt";

declare global {
    const HINTKEEP_API_URL: string;
    const HINTKEEP_ENVIRONMENT_TYPE: "development" | "staging" | "production";
}

export interface IAppProps {
    configure?(dependencyContainer: DependencyContainer, router: ReturnType<typeof createBrowserRouter>): DependencyContainer;
}

export function App({ configure }: IAppProps): React.JSX.Element {
    const appRouter = useAppRouter();

    return (
        <ModalContextProvider>
            <ConfirmationPromptProvider>
                <IndexedDatabaseContextProvider databaseDefinition={HintKeepDatabaseDefinition}>
                    <HintKeepDependencyContainerProvider
                        router={appRouter}
                        configure={configure}
                    >
                        <RouterProvider router={appRouter} />

                        <ModalsDisplay />
                        <ConfirmationPrompt />
                        <aside className="global-notifications">
                            <GlobalNotificationsContainer />
                        </aside>
                    </HintKeepDependencyContainerProvider>
                </IndexedDatabaseContextProvider>
            </ConfirmationPromptProvider>
        </ModalContextProvider>
    );
}