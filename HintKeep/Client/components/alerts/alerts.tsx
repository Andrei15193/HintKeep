import React from 'react';
import { Alert } from './alert';
import { useViewModel } from '../use-view-model';

export function Alerts(): JSX.Element {
    const $vm = useViewModel(({ alertsViewModel }) => alertsViewModel);

    return (
        <>
            {$vm.alerts.map((alert, index) => <Alert key={index} $vm={alert} />)}
        </>
    );
}