import { ViewModel } from './core';
import { alertsStore } from '../stores';
import { Alert } from '../stores/alerts-store';

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

export class AlertViewModel extends ViewModel {
    private readonly _alert: Alert;

    public constructor(alert: Alert) {
        super(alertsStore);
        this._alert = alert;
    }

    public get message(): string {
        return this._alert.message;
    }

    public dismiss(): void {
        alertsStore.remove(this._alert);
    }
}