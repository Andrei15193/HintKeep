import { ViewModel } from './core';
import { alertsStore } from '../stores';
import { Alert } from '../stores/alert';
import { AlertViewModel } from './alert-view-model';

export class AlertsViewModel extends ViewModel {
    private _alerts: readonly AlertViewModel[];

    public constructor() {
        super(alertsStore);
        this._alerts = this._mapAlerts(alertsStore.alerts);
    }

    public get alerts(): readonly AlertViewModel[] {
        return this._alerts;
    }

    protected storeChanged(): void {
        this._alerts = this._mapAlerts(alertsStore.alerts);
        this.notifyPropertyChanged('alerts');
    }

    private _mapAlerts(alerts: readonly Alert[]): readonly AlertViewModel[] {
        return alerts.map(alert => new AlertViewModel(alert));
    }
}