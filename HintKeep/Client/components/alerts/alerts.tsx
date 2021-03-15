import React from 'react';
import { Alert } from './alert';
import { AlertsViewModel } from '../../view-models/alerts-view-model';
import { useViewModel } from '../view-model-wrappers';

export function Alerts(): JSX.Element {
    const $vm = useViewModel(AlertsViewModel);

    return (
        <>
            {$vm.alerts.map((alert, index) => <Alert key={index} $vm={alert} />)}
        </>
    );
}