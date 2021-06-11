import React, { useEffect } from 'react';
import { useLocation, useHistory } from 'react-router-dom';
import { watchEvent } from 'react-model-view-viewmodel';
import { useViewModel } from '../use-view-model';
import { AuthenticationViewModel } from '../../view-models/authentication-view-model';
import { Spinner } from '../loaders';

export function Authentication(): JSX.Element {
    const { hash } = useLocation();
    const { push } = useHistory();
    const $vm = useViewModel(({ axios }) => new AuthenticationViewModel(axios));

    watchEvent($vm.authenticated, () => push('/'));
    useEffect(() => $vm.authenticate(hash.substring('#id_token='.length)), []);

    return <Spinner />
}