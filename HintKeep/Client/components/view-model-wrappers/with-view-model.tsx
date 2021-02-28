import type { ComponentType } from 'react'
import type { ElementFactoryCallback, ElementWithPropsFactoryCallback, ViewModelType } from './common';
import type { IObserver } from '../../Observer';
import type { ViewModel } from '../../view-models/core/view-model';
import { useEffect, useState } from 'react';

export interface IWithViewModelProps<TViewModel extends ViewModel> {
    viewModelType: ViewModelType<TViewModel>,
    children: ElementFactoryCallback<TViewModel>
}

export function WithViewModel<TViewModel extends ViewModel>({ viewModelType, children }: IWithViewModelProps<TViewModel>): JSX.Element {
    const [{ $vm }, setState] = useState(() => ({ $vm: new viewModelType(), lastUpdated: new Date() }));
    useEffect(
        () => {
            let isMounted = true;
            const viewModelObserver: IObserver = {
                notifyChanged(): void {
                    if (isMounted)
                        setState({ $vm, lastUpdated: new Date() });
                }
            };
            $vm.subscribe(viewModelObserver);
            return () => {
                $vm.unsubscribe(viewModelObserver);
                isMounted = false;
            }
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
                let isMounted = true;
                const viewModelObserver: IObserver = {
                    notifyChanged(): void {
                        if (isMounted)
                            setState({ $vm, lastUpdated: new Date() });
                    }
                };
                $vm.subscribe(viewModelObserver);
                return () => {
                    $vm.unsubscribe(viewModelObserver);
                    isMounted = false;
                }
            },
            []
        );

        return elementFactoryCallback($vm, componentProps);
    }
}