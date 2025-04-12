import { useMemo } from "react";
import { DependencyContainer } from "react-model-view-viewmodel";
import { useIndexedDatabase, IndexedDatabase } from "../Data/IndexedDatabase";
import { useUser } from "../Pages/Contexts/UserContext";
import { User } from "../Pages/Model/IUser";

export function useHintKeepDependencyContainer(): DependencyContainer {
    const user = useUser();
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

            return dependencyContainer;
        },
        [user, database]
    );
}