import type { PropsWithChildren } from 'react'
import { PropsWithViewModel, watchViewModel } from './../view-model-wrappers';
import React from 'react';
import { Spinner } from './spinner';
import { ApiViewModel, ApiViewModelState } from '../../view-models/core/';

export function BusyContent({ $vm, children }: PropsWithChildren<PropsWithViewModel<ApiViewModel>>): JSX.Element {
    watchViewModel($vm);

    return $vm.state === ApiViewModelState.Busy ? <Spinner /> : <>{children}</>
}