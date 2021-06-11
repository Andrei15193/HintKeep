import type { PropsWithChildren } from 'react';
import type { AuthenticationGuardViewModel } from '../../view-models/authentication-guard-view-model';
import React, { useEffect } from 'react';
import { watchViewModel } from 'react-model-view-viewmodel';
import { Spinner } from '../loaders';

export interface IAuthenticationGuardProps {
    readonly $vm: AuthenticationGuardViewModel;
}

export function AuthenticationGuard({ $vm, children }: PropsWithChildren<IAuthenticationGuardProps>): JSX.Element {
    watchViewModel($vm);

    useEffect(() => { $vm.ensureAuthenticatedAsync() }, []);

    return $vm.isAuthenticationEnsured ? <>{children}</> : <Spinner />;
}