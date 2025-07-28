import { useMemo } from "react";
import { DependencyContainer } from "react-model-view-viewmodel";
import { useAuthentication } from "../Contexts/AuthenticationContext";
import { useIndexedDatabase, IndexedDatabase } from "../Data/IndexedDatabase";
import { HintKeepFormField } from "../Forms/ViewModels";
import { User } from "../Models";
import { Notifications } from "../Notifications";
import { AccountsSearchTextFieldToken, ArchivedAccountsSearchTextFieldToken } from "./DependencyTokens";

export function useHintKeepDependencyContainer(): DependencyContainer {
    const { user } = useAuthentication();
    const { database } = useIndexedDatabase();

    return useMemo(
        () => {
            const dependencyContainer = new DependencyContainer();

            if (database === null) {
            }
            else {
                dependencyContainer.registerInstanceToToken(IndexedDatabase, database);
            }

            dependencyContainer.registerInstanceToToken(User, user);
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

            return dependencyContainer;
        },
        [user, database]
    );
}