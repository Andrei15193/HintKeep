import React, { type PropsWithChildren } from "react";
import { DependencyResolverProvider } from "react-model-view-viewmodel";
import { useHintKeepDependencyContainer } from "./UseHintKeepDependencyContainer";

export function HintKeepDependencyContainerProvider({ children }: PropsWithChildren<{}>): React.JSX.Element {
    const hintKeepDependencyContainer = useHintKeepDependencyContainer();

    return (
        <DependencyResolverProvider
            dependencyResolver={hintKeepDependencyContainer}
            children={children}
        />
    );
}