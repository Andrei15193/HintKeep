import type { IEventHandler, INotifyPropertyChanged } from '../../events';
import { useEffect, useState } from 'react';

export type PropsWithViewModel<TViewModel, TOther = {}> = TOther & { $vm: TViewModel };

export function watchViewModel<TViewModel extends INotifyPropertyChanged>($vm: TViewModel, watchedProperties?: readonly (keyof TViewModel)[]) {
    const [_, setState] = useState({});

    if (!watchedProperties || watchedProperties.length > 0)
        useEffect(
            () => {
                let vmProps: any = {};
                let propertyChangedEventHandler: IEventHandler<readonly string[]> = watchedProperties
                    ? {
                        handle(subject: any, changedProperties: readonly string[]): void {
                            const watchedChangedProperties = changedProperties.filter(changedProperty => watchedProperties.includes(changedProperty as keyof TViewModel));
                            if (watchedChangedProperties.length > 0) {
                                vmProps = Object.assign({}, vmProps, watchedChangedProperties.reduce((result, propertyName) => Object.assign(result, { [propertyName]: subject[propertyName] }), {}));
                                setState(vmProps);
                            }
                        }
                    }
                    : {
                        handle(subject: any, changedProperties: readonly string[]): void {
                            vmProps = Object.assign({}, vmProps, changedProperties.reduce((result, propertyName) => Object.assign(result, { [propertyName]: subject[propertyName] }), {}));
                            setState(vmProps);
                        }
                    };
                $vm.propertyChanged.subscribe(propertyChangedEventHandler);
                return () => { $vm.propertyChanged.unsubscribe(propertyChangedEventHandler); }
            },
            watchedProperties ? [$vm, ...watchedProperties] : [$vm]
        );
}