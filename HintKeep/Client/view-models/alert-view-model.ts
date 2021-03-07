import { ViewModel } from './core';
import { alertsStore } from '../stores';
import { Alert } from '../stores/alert';

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