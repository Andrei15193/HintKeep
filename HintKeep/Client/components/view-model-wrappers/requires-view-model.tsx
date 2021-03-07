import type { ComponentType } from 'react';
import type { ElementWithPropsFactoryCallback } from './common';
import type { IEventHandler, INotifyPropertyChanged } from '../../events';
import { useEffect, useState } from 'react';

export type PropsWithViewModel<TViewModel, TOther = {}> = TOther & { $vm: TViewModel };

export function requiresViewModel<TViewModel extends INotifyPropertyChanged, TComponentProps = {}>(elementFactoryCallback: ElementWithPropsFactoryCallback<TViewModel, TComponentProps>): ComponentType<PropsWithViewModel<TViewModel, TComponentProps>> {
    return function requiresViewModel(componentProps: PropsWithViewModel<TViewModel, TComponentProps>): JSX.Element {
        const { $vm } = componentProps;
        const [_, setState] = useState(() => ({ lastUpdated: new Date() }));
        useEffect(
            () => {
                const propertyChangedEventHandler: IEventHandler<readonly string[]> = {
                    handle(): void {
                        setState({ lastUpdated: new Date() });
                    }
                };
                $vm.propertyChanged.subscribe(propertyChangedEventHandler);
                return () => { $vm.propertyChanged.unsubscribe(propertyChangedEventHandler); }
            },
            [$vm]
        );

        return elementFactoryCallback($vm, componentProps);
    }
}