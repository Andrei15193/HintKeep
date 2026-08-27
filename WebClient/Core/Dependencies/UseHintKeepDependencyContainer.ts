import type { createBrowserRouter } from "react-router";
import { useMemo } from "react";
import { DependencyContainer } from "react-model-view-viewmodel";
import { useWindow } from "../../Pages/WindowContext";
import { CurrentUser, CurrentUserProvider, UserHandler } from "../Authentication";
import { StorageContext, StorageContextService } from "../Data";
import { IndexedDatabaseProvider, IndexedDatabaseHandler, IndexedDatabaseHandlerService } from "../Data/IndexedDatabase";
import { HintKeepDatabaseDefinition } from "../Data/IndexedDatabase/HintKeep";
import { HintKeepFormField } from "../Forms/ViewModels";
import { Notifications } from "../Notifications";
import { AccountsSearchTextFieldToken, ArchivedAccountsSearchTextFieldToken } from "./DependencyTokens";

export function useHintKeepDependencyContainer(router: ReturnType<typeof createBrowserRouter>, configure?: (dependencyContainer: DependencyContainer, router: ReturnType<typeof createBrowserRouter>) => DependencyContainer): DependencyContainer {
    const window = useWindow();

    return useMemo(
        () => {
            const dependencyContainer = new DependencyContainer();

            dependencyContainer.registerSingletonType(Notifications);

            dependencyContainer.registerInstanceToToken(StorageContext, new StorageContextService("HintKeepAPI"));

            dependencyContainer.registerSingletonType(UserHandler);
            dependencyContainer.registerSingletonFactoryToToken(CurrentUserProvider, ({ resolve }) => resolve(UserHandler));
            dependencyContainer.registerTransientFactoryToToken(CurrentUser, ({ resolve }) => resolve(CurrentUserProvider).user);

            dependencyContainer.registerSingletonFactoryToToken(IndexedDatabaseHandler, () => new IndexedDatabaseHandlerService(HintKeepDatabaseDefinition, window));
            dependencyContainer.registerSingletonFactoryToToken(IndexedDatabaseProvider, ({ resolve }) => resolve(IndexedDatabaseHandler));

            dependencyContainer.registerScopedFactoryToToken(AccountsSearchTextFieldToken, () => new HintKeepFormField<string>({
                name: "search",
                label: "Search",
                initialValue: ""
            }));
            dependencyContainer.registerScopedFactoryToToken(ArchivedAccountsSearchTextFieldToken, () => new HintKeepFormField<string>({
                name: "search",
                label: "Search",
                initialValue: ""
            }));

            if (configure)
                configure(dependencyContainer, router);

            return dependencyContainer;
        },
        [window, router, configure]
    );
}