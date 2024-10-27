import { Alert } from './alert';
import { useObservableCollection, useViewModelDependency } from 'react-model-view-viewmodel';
import { AlertsViewModel } from '../../view-models/alerts-view-model';

export function Alerts(): JSX.Element {
    const alertsViewModel = useViewModelDependency(AlertsViewModel);
    useObservableCollection(alertsViewModel.alerts);

    return (
        <>
            {alertsViewModel.alerts.map((alert, index) => <Alert key={index} alertViewModel={alert} />)}
        </>
    );
}