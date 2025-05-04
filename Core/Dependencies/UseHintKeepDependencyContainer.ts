import { useMemo } from "react";
import { DependencyContainer } from "react-model-view-viewmodel";
import { useAuthentication } from "../Contexts/AuthenticationContext";
import { useIndexedDatabase, IndexedDatabase } from "../Data/IndexedDatabase";
import { User } from "../Models";
import { Notifications } from "../Notifications";

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
            dependencyContainer.registerSingletonType(Notifications);

            return dependencyContainer;
        },
        [user, database]
    );
}