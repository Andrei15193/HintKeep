import React from 'react';
import { Alert } from './alert';
import { AlertsViewModel } from '../../view-models/alerts-view-model';
import { WithViewModel } from '../view-model-wrappers';

export function Alerts(): JSX.Element {
    return (
        <WithViewModel viewModelType={AlertsViewModel}>{$vm => <>
            {$vm.alerts.map((alert, index) => <Alert key={index} $vm={alert} />)}
        </>}</WithViewModel>
    );
}