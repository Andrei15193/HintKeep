import React, { type PropsWithChildren } from "react";
import { type DependencyContainer, DependencyResolverProvider } from "react-model-view-viewmodel";
import { useHintKeepDependencyContainer } from "./UseHintKeepDependencyContainer";

export interface IHintKeepDependencyContainerProviderProps extends PropsWithChildren {
    configure?(dependencyContainer: DependencyContainer): DependencyContainer;
}

export function HintKeepDependencyContainerProvider({ children, configure }: IHintKeepDependencyContainerProviderProps): React.JSX.Element {
    const hintKeepDependencyContainer = useHintKeepDependencyContainer(configure);

    return (
        <DependencyResolverProvider
            dependencyResolver={hintKeepDependencyContainer}
            children={children}
        />
    );
}