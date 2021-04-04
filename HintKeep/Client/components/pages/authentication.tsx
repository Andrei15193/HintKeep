import React, { useEffect } from 'react';
import { useLocation, useHistory } from 'react-router-dom';
import { AuthenticationViewModel } from '../../view-models/authentication-view-model';
import { Spinner } from '../loaders';
import { useViewModel, watchEvent } from '../view-model-hooks';

export function Authentication(): JSX.Element {
    const { hash } = useLocation();
    const { push } = useHistory();
    const $vm = useViewModel(AuthenticationViewModel);

    watchEvent($vm.authenticated, () => push('/'));
    useEffect(() => $vm.authenticate(hash.substring('#id_token='.length)), []);

    return <Spinner />
}