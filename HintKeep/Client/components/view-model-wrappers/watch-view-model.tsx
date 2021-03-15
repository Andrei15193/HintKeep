import type { IEventHandler } from '../../events';
import type { ViewModel } from '../../view-models/core';
import { useEffect, useState } from 'react';

export type PropsWithViewModel<TViewModel, TOther = {}> = TOther & { $vm: TViewModel };

export function watchViewModel($vm: ViewModel) {
    const [_, setState] = useState({});

    useEffect(
        () => {
            let vmProps: any = {};
            const propertyChangedEventHandler: IEventHandler<readonly string[]> = {
                handle(subject: any, changedProperties: readonly string[]): void {
                    vmProps = Object.assign(vmProps, changedProperties.reduce((result, propertyName) => Object.assign(result, { [propertyName]: subject[propertyName] }), {}));
                    setState(vmProps);
                }
            };
            $vm.propertyChanged.subscribe(propertyChangedEventHandler);
            return () => { $vm.propertyChanged.unsubscribe(propertyChangedEventHandler); }
        },
        [$vm]
    );
}