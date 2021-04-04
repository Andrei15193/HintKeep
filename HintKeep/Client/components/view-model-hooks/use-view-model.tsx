import type { ViewModelType } from './common';
import type { ViewModel } from '../../view-models/core/view-model';
import type { IEventHandler } from '../../events';
import { useState, useEffect } from 'react';

export function useViewModel<TViewModel extends ViewModel>(viewModelType: ViewModelType<TViewModel>, watchedProperties?: readonly (keyof TViewModel)[]): TViewModel {
    const [{ $vm }, setState] = useState(() => ({ $vm: new viewModelType(), vmProps: {} }));

    if (!watchedProperties || watchedProperties.length > 0)
        useEffect(
            () => {
                let vmProps: any = {};
                const propertyChangedEventHandler: IEventHandler<readonly string[]> = watchedProperties
                    ? {
                        handle(subject: any, changedProperties: readonly string[]): void {
                            const watchedChangedProperties = changedProperties.filter(changedProperty => watchedProperties.includes(changedProperty as keyof TViewModel));
                            if (watchedChangedProperties.length > 0) {
                                vmProps = Object.assign({}, vmProps, changedProperties.reduce((result, propertyName) => Object.assign(result, { [propertyName]: subject[propertyName] }), {}));
                                setState({ $vm, vmProps });
                            }
                        }
                    }
                    : {
                        handle(subject: any, changedProperties: readonly string[]): void {
                            vmProps = Object.assign({}, vmProps, changedProperties.reduce((result, propertyName) => Object.assign(result, { [propertyName]: subject[propertyName] }), {}));
                            setState({ $vm, vmProps });
                        }
                    };
                $vm.propertyChanged.subscribe(propertyChangedEventHandler);
                return () => $vm.propertyChanged.unsubscribe(propertyChangedEventHandler);
            },
            watchedProperties ? [...watchedProperties] : []
        );

    return $vm;
}