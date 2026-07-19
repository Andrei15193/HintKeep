import { useMemo } from "react";
import { DependencyContainer } from "react-model-view-viewmodel";
import { CurrentUser, CurrentUserProvider, UserHandler } from "../Authentication";
import { useIndexedDatabase, IndexedDatabase } from "../Data/IndexedDatabase";
import { HintKeepFormField } from "../Forms/ViewModels";
import { Notifications } from "../Notifications";
import { AccountsSearchTextFieldToken, ArchivedAccountsSearchTextFieldToken } from "./DependencyTokens";

export function useHintKeepDependencyContainer(configure?: (dependencyContainer: DependencyContainer) => DependencyContainer): DependencyContainer {
    const { database } = useIndexedDatabase();

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

            dependencyContainer.registerSingletonType(Notifications);

            dependencyContainer.registerSingletonType(UserHandler);
            dependencyContainer.registerSingletonFactoryToToken(CurrentUserProvider, ({ resolve }) => resolve(UserHandler));
            dependencyContainer.registerTransientFactoryToToken(CurrentUser, ({ resolve }) => resolve(CurrentUserProvider).user);

            if (configure)
                configure(dependencyContainer);

            return dependencyContainer;
        },
        [database, configure]
    );
}