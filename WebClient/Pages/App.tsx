import type { DependencyContainer } from "react-model-view-viewmodel";
import React from "react";
import { RouterProvider } from "react-router";
import { IndexedDatabaseProvider } from "../Core/Data/IndexedDatabase";
import { HintKeepDatabaseDefinition } from "../Core/Data/IndexedDatabase/HintKeep";
import { HintKeepDependencyContainerProvider } from "../Core/Dependencies";
import { ModalContextProvider, ModalsDisplay } from "../Core/Modals";
import { GlobalNotificationsContainer } from "../Core/Notifications";
import { ConfirmationPromptProvider } from "../Core/Prompt";
import { useAppRouter } from "./AppRouter";
import { ConfirmationPrompt } from "./ConfirmationPrompt";

declare global {
    const HINTKEEP_API_URL: string;
    const HINTKEEP_ENVIRONMENT_TYPE: string;
}

export interface IAppProps {
    configure?(dependencyContainer: DependencyContainer): DependencyContainer;
}

export function App({ configure }: IAppProps): React.JSX.Element {
    const appRouter = useAppRouter();

    return (
        <ModalContextProvider>
            <ConfirmationPromptProvider>
                <IndexedDatabaseProvider databaseDefinition={HintKeepDatabaseDefinition}>
                    <HintKeepDependencyContainerProvider configure={configure}>
                        <RouterProvider router={appRouter} />

                        <ModalsDisplay />
                        <ConfirmationPrompt />
                        <aside className="global-notifications">
                            <GlobalNotificationsContainer />
                        </aside>
                    </HintKeepDependencyContainerProvider>
                </IndexedDatabaseProvider>
            </ConfirmationPromptProvider>
        </ModalContextProvider>
    );
}