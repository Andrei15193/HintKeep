import type { createBrowserRouter } from "react-router";
import React, { type PropsWithChildren } from "react";
import { type DependencyContainer, DependencyResolverProvider } from "react-model-view-viewmodel";
import { useHintKeepDependencyContainer } from "./UseHintKeepDependencyContainer";

export interface IHintKeepDependencyContainerProviderProps extends PropsWithChildren {
    readonly router: ReturnType<typeof createBrowserRouter>;

    configure?(dependencyContainer: DependencyContainer, router: ReturnType<typeof createBrowserRouter>): DependencyContainer;

}

export function HintKeepDependencyContainerProvider({ configure, router, children }: IHintKeepDependencyContainerProviderProps): React.JSX.Element {
    const hintKeepDependencyContainer = useHintKeepDependencyContainer(router, configure);

    return (
        <DependencyResolverProvider
            dependencyResolver={hintKeepDependencyContainer}
            children={children}
        />
    );
}