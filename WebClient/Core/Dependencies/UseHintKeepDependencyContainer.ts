import { useMemo } from "react";
import { DependencyContainer } from "react-model-view-viewmodel";
import { useWindow } from "../../Pages/WindowContext";
import { CurrentUser, CurrentUserProvider, UserHandler } from "../Authentication";
import { useIndexedDatabase, IndexedDatabase, IndexedDatabaseProvider, IndexedDatabaseHandler, IndexedDatabaseHandlerService } from "../Data/IndexedDatabase";
import { HintKeepDatabaseDefinition } from "../Data/IndexedDatabase/HintKeep";
import { HintKeepFormField } from "../Forms/ViewModels";
import { Notifications } from "../Notifications";
import { AccountsSearchTextFieldToken, ArchivedAccountsSearchTextFieldToken } from "./DependencyTokens";

export function useHintKeepDependencyContainer(configure?: (dependencyContainer: DependencyContainer) => DependencyContainer): DependencyContainer {
    const { database } = useIndexedDatabase();
    const window = useWindow();

    return useMemo(
        () => {
            const dependencyContainer = new DependencyContainer();

            if (database === null) {
            }
            else {
                dependencyContainer.registerInstanceToToken(IndexedDatabase, database);
            }

            dependencyContainer.registerInstanceToToken(AccountsSearchTextFieldToken, new HintKeepFormField<string>({
                name: "search",
                label: "Search",
                initialValue: ""
            }));
            dependencyContainer.registerInstanceToToken(ArchivedAccountsSearchTextFieldToken, new HintKeepFormField<string>({
                name: "search",
                label: "Search",
                initialValue: ""
            }));

            dependencyContainer.registerSingletonFactoryToToken(IndexedDatabaseHandler, () => new IndexedDatabaseHandlerService(HintKeepDatabaseDefinition, window));
            dependencyContainer.registerSingletonFactoryToToken(IndexedDatabaseProvider, ({ resolve }) => resolve(IndexedDatabaseHandler));

            dependencyContainer.registerSingletonType(Notifications);

            dependencyContainer.registerSingletonType(UserHandler);
            dependencyContainer.registerSingletonFactoryToToken(CurrentUserProvider, ({ resolve }) => resolve(UserHandler));
            dependencyContainer.registerTransientFactoryToToken(CurrentUser, ({ resolve }) => resolve(CurrentUserProvider).user);

            if (configure)
                configure(dependencyContainer);

            return dependencyContainer;
        },
        [database, window, configure]
    );
}