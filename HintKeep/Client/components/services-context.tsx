import type { PropsWithChildren } from 'react';
import { useMemo } from 'react';
import { Axios, AxiosInstance } from '../services';
import { AlertsViewModel } from '../view-models/alerts-view-model';
import { SessionViewModel } from '../view-models/session-view-model';
import { DependencyContainer, DependencyResolverProvider } from 'react-model-view-viewmodel';

export function ServicesProvider({ children }: PropsWithChildren<{}>): JSX.Element {
    const dependencyContainer = useMemo(
        () => new DependencyContainer()
            .registerInstanceToToken(Axios, AxiosInstance)
            .registerSingletonType(AlertsViewModel)
            .registerSingletonType(SessionViewModel),
        []
    );

    return (
        <DependencyResolverProvider
            dependencyResolver={dependencyContainer}
            children={children} />
    );
}