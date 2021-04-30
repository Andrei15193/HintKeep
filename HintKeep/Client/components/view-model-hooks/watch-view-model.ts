import type { IEventHandler, INotifyPropertyChanged } from '../../events';
import { useEffect, useState } from 'react';

export type PropsWithViewModel<TViewModel, TOther = {}> = TOther & { $vm: TViewModel };

export function watchViewModel<TViewModel extends INotifyPropertyChanged>($vm: TViewModel, watchedProperties?: readonly (keyof TViewModel)[]) {
    const [_, setState] = useState({});

    useEffect(
        () => {
            let vmProps: any = {};
            if (!watchedProperties || watchedProperties.length > 0) {
                const propertyChangedEventHandler: IEventHandler<readonly string[]> = watchedProperties
                    ? {
                        handle(subject: any, changedProperties: readonly string[]): void {
                            setVmProps(subject, changedProperties.filter(changedProperty => watchedProperties.includes(changedProperty as keyof TViewModel)));
                        }
                    }
                    : {
                        handle: setVmProps
                    };
                $vm.propertyChanged.subscribe(propertyChangedEventHandler);
                return () => $vm.propertyChanged.unsubscribe(propertyChangedEventHandler);
            }

            function setVmProps(subject: any, properties: readonly string[]): void {
                vmProps = properties.reduce(
                    (result, propertyName) => {
                        result[propertyName] = subject[propertyName];
                        return result;
                    },
                    Object.assign({}, vmProps)
                );
                setState(vmProps);
            }
        },
        watchedProperties ? [$vm, ...watchedProperties] : [$vm]
    );
}