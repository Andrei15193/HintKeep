import type { ViewModelType } from './common';
import type { ViewModel } from '../../view-models/core/view-model';
import type { IEventHandler } from '../../events';
import { useState, useEffect } from 'react';

export function useViewModel<TViewModel extends ViewModel>(viewModelType: ViewModelType<TViewModel>): TViewModel {
    const [{ $vm }, setState] = useState(() => ({ $vm: new viewModelType(), vmProps: {} }));

    useEffect(
        () => {
            let vmProps: any = {};
            const propertyChangedEventHandler: IEventHandler<readonly string[]> = {
                handle(subject: any, changedProperties: readonly string[]): void {
                    vmProps = Object.assign(vmProps, changedProperties.reduce((result, propertyName) => Object.assign(result, { [propertyName]: subject[propertyName] }), {}));
                    setState({ $vm, vmProps });
                }
            };
            $vm.propertyChanged.subscribe(propertyChangedEventHandler);
            return () => $vm.propertyChanged.unsubscribe(propertyChangedEventHandler);
        },
        []
    );

    return $vm;
}