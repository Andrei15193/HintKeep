import { Store } from './store';
import { Alert } from './alert';

export class AlertsStore extends Store {
    private readonly _alerts: Alert[] = [];

    public get alerts(): readonly Alert[] {
        return this._alerts;
    }

    public addAlert(message: string): void {
        this._alerts.push(new Alert(message));
        this.notifyPropertyChanged('alerts');
    }

    public remove(alert: Alert): void {
        const alertIndex = this._alerts.indexOf(alert);
        if (alertIndex >= 0) {
            this._alerts.splice(alertIndex, 1);
            this.notifyPropertyChanged('alerts');
        }
    }
}