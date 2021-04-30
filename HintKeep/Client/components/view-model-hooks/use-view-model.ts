import type { ViewModel } from '../../view-models/core/view-model';
import type { IEventHandler, INotifyPropertyChanged } from '../../events';
import { useState, useEffect } from 'react';

export type ViewModelType<TViewModel extends INotifyPropertyChanged> = {
    new(): TViewModel;
};

export function useViewModel<TViewModel extends ViewModel>(viewModelType: ViewModelType<TViewModel>, watchedProperties?: readonly (keyof TViewModel)[]): TViewModel {
    const [{ $vm }, setState] = useState(() => ({ $vm: new viewModelType(), vmProps: {} }));

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
                setState({ $vm, vmProps });
            }
        },
        watchedProperties ? [...watchedProperties] : []
    );

    return $vm;
}