import type { PropsWithChildren } from 'react'
import type { PropsWithViewModel } from './../view-model-wrappers';
import React, { memo } from 'react';
import { Spinner } from './spinner';
import { requiresViewModel } from './../view-model-wrappers';
import { ApiViewModel, ApiViewModelState } from '../../view-models/core/';

export const BusyContent: React.ComponentType<PropsWithChildren<PropsWithViewModel<ApiViewModel>>> = memo(
    requiresViewModel(($vm, { children }) => $vm.state === ApiViewModelState.Busy ? <Spinner /> : <>{children}</>),
    () => false
);