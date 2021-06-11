import { ViewModel } from 'react-model-view-viewmodel';

export class AlertsViewModel extends ViewModel {
    private readonly _alerts: AlertViewModel[] = [];

    public get alerts(): readonly AlertViewModel[] {
        return this._alerts;
    }

    public addAlert(message: string): void {
        this._alerts.push(new AlertViewModel(message, this));
        this.notifyPropertiesChanged('alerts');
    }

    public remove(alert: AlertViewModel): void {
        const alertIndex = this._alerts.indexOf(alert);
        if (alertIndex >= 0) {
            this._alerts.splice(alertIndex, 1);
            this.notifyPropertiesChanged('alerts');
        }
    }
}

export class AlertViewModel extends ViewModel {
    private readonly _message: string;
    private readonly _alertsViewModel: AlertsViewModel;

    public constructor(message: string, alertsViewModel: AlertsViewModel) {
        super();
        this._message = message;
        this._alertsViewModel = alertsViewModel;
    }

    public get message(): string {
        return this._message;
    }

    public dismiss(): void {
        this._alertsViewModel.remove(this);
    }
}