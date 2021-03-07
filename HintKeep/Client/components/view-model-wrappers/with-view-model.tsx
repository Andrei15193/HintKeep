import type { ComponentType } from 'react'
import type { ElementFactoryCallback, ElementWithPropsFactoryCallback, ViewModelType } from './common';
import type { ViewModel } from '../../view-models/core/view-model';
import { useEffect, useState } from 'react';
import { IEventHandler } from '../../events';

export interface IWithViewModelProps<TViewModel extends ViewModel> {
    viewModelType: ViewModelType<TViewModel>,
    children: ElementFactoryCallback<TViewModel>
}

export function WithViewModel<TViewModel extends ViewModel>({ viewModelType, children }: IWithViewModelProps<TViewModel>): JSX.Element {
    const [{ $vm }, setState] = useState(() => ({ $vm: new viewModelType(), lastUpdated: new Date() }));
    useEffect(
        () => {
            const propertyChangedEventHandler: IEventHandler<readonly string[]> = {
                handle(): void {
                    setState({ $vm, lastUpdated: new Date() });
                }
            };
            $vm.propertyChanged.subscribe(propertyChangedEventHandler);
            return () => $vm.propertyChanged.unsubscribe(propertyChangedEventHandler);
        },
        []
    );

    return children($vm);
}

export function withViewModel<TViewModel extends ViewModel, TComponentProps = {}>(ViewModelType: ViewModelType<TViewModel>, elementFactoryCallback: ElementWithPropsFactoryCallback<TViewModel, TComponentProps>): ComponentType<TComponentProps> {
    return function withViewModel(componentProps: TComponentProps): JSX.Element {
        const [{ $vm }, setState] = useState(() => ({ $vm: new ViewModelType(), lastUpdated: new Date() }));
        useEffect(
            () => {
                const propertyChangedEventHandler: IEventHandler<readonly string[]> = {
                    handle(): void {
                        setState({ $vm, lastUpdated: new Date() });
                    }
                };
                $vm.propertyChanged.subscribe(propertyChangedEventHandler);
                return () => $vm.propertyChanged.unsubscribe(propertyChangedEventHandler);
            },
            []
        );

        return elementFactoryCallback($vm, componentProps);
    }
}