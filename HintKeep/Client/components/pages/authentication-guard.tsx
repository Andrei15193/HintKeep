import type { PropsWithChildren } from 'react';
import type { PropsWithViewModel } from '../view-model-hooks';
import type { AuthenticationGuardViewModel } from '../../view-models/authentication-guard-view-model';
import React, { useEffect } from 'react';
import { Spinner } from '../loaders';
import { watchViewModel } from '../view-model-hooks';

export function AuthenticationGuard({ $vm, children }: PropsWithChildren<PropsWithViewModel<AuthenticationGuardViewModel>>): JSX.Element {
    watchViewModel($vm);

    useEffect(() => { $vm.ensureAuthenticatedAsync() }, []);

    return $vm.isAuthenticationEnsured ? <>{children}</> : <Spinner />;
}