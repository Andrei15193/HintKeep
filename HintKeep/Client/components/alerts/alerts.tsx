import React from 'react';
import { Alert } from './alert';
import { requireViewModel } from '../use-view-model';
import { watchCollection } from 'react-model-view-viewmodel';

export function Alerts(): JSX.Element {
    const $vm = requireViewModel(({ alertsViewModel }) => alertsViewModel);
    watchCollection($vm.alerts);

    return (
        <>
            {$vm.alerts.map((alert, index) => <Alert key={index} $vm={alert} />)}
        </>
    );
}