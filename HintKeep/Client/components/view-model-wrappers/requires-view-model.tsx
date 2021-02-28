import type { ComponentType } from 'react';
import type { ElementWithPropsFactoryCallback } from './common';
import type { IObservable, IObserver } from '../../observer';
import { useEffect, useState } from 'react';

export type PropsWithViewModel<TViewModel, TOther = {}> = TOther & { $vm: TViewModel };

export function requiresViewModel<TViewModel extends IObservable, TComponentProps = {}>(elementFactoryCallback: ElementWithPropsFactoryCallback<TViewModel, TComponentProps>): ComponentType<PropsWithViewModel<TViewModel, TComponentProps>> {
    return function requiresViewModel(componentProps: PropsWithViewModel<TViewModel, TComponentProps>): JSX.Element {
        const { $vm } = componentProps;
        const [_, setState] = useState(() => ({ lastUpdated: new Date() }));
        useEffect(
            () => {
                let isMounted = true;
                const viewModelObserver: IObserver = {
                    notifyChanged(): void {
                        if (isMounted)
                            setState({ lastUpdated: new Date() });
                    }
                };
                $vm.subscribe(viewModelObserver);
                return () => {
                    $vm.unsubscribe(viewModelObserver);
                    isMounted = false;
                }
            },
            [$vm]
        );

        return elementFactoryCallback($vm, componentProps);
    }
}