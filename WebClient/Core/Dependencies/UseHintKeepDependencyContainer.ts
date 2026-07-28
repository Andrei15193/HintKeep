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

export function useHintKeepDependencyContainer(configure?: (dependencyContainer: DependencyContainer) => DependencyContainer): DependencyContainer {
    const window = useWindow();

    return useMemo(
        () => {
            const dependencyContainer = new DependencyContainer();

            dependencyContainer.registerSingletonType(Notifications);

            dependencyContainer.registerSingletonType(UserHandler);
            dependencyContainer.registerSingletonFactoryToToken(CurrentUserProvider, ({ resolve }) => resolve(UserHandler));
            dependencyContainer.registerTransientFactoryToToken(CurrentUser, ({ resolve }) => resolve(CurrentUserProvider).user);

            dependencyContainer.registerInstanceToToken(StorageContext, new StorageContextService("HintKeepAPI"));

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
                configure(dependencyContainer);

            return dependencyContainer;
        },
        [window, configure]
    );
}