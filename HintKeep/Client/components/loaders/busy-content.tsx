import type { PropsWithChildren } from 'react'
import type { ApiViewModel } from '../../view-models/api-view-model';
import { watchViewModel } from 'react-model-view-viewmodel';
import React from 'react';
import { Spinner } from './spinner';
import { ApiViewModelState } from '../../view-models/api-view-model';

export interface IBusyContentProps {
    readonly $vm: ApiViewModel;
}

export function BusyContent({ $vm, children }: PropsWithChildren<IBusyContentProps>): JSX.Element {
    watchViewModel($vm, ['state']);

    return $vm.state === ApiViewModelState.Busy ? <Spinner /> : <>{children}</>
}