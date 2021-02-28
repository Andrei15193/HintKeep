import type { PropsWithChildren } from 'react'
import type { PropsWithViewModel } from './../view-model-wrappers';
import React, { memo } from 'react';
import { Spinner } from './spinner';
import { requiresViewModel } from './../view-model-wrappers';
import { AsyncViewModel, AsyncViewModelState } from '../../view-models/core/';

export const BusyContent: React.ComponentType<PropsWithChildren<PropsWithViewModel<AsyncViewModel>>> = memo(
    requiresViewModel(($vm, { children }) => $vm.state === AsyncViewModelState.Busy ? <Spinner /> : <>{children}</>),
    () => false
);