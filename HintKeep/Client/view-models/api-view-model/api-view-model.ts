import type { AxiosInstance, AxiosRequestConfig } from 'axios';
import type { AlertsViewModel } from '../alerts-view-model';
import { ViewModel } from 'react-model-view-viewmodel';
import { RequestHandlerBuilder } from './request-handler-builder';

export enum ApiViewModelState {
    Ready,
    Busy,
    Faulted
}

export abstract class ApiViewModel extends ViewModel {
    private readonly _axios: AxiosInstance;
    private readonly _alertsViewModel: AlertsViewModel;
    private _state: ApiViewModelState = ApiViewModelState.Ready;

    public constructor(axios: AxiosInstance, alertsViewModel: AlertsViewModel) {
        super();
        this._axios = axios;
        this._alertsViewModel = alertsViewModel;
    }

    public get state(): ApiViewModelState {
        return this._state;
    }

    protected get alertsViewModel(): AlertsViewModel {
        return this._alertsViewModel;
    }

    private _setState(value: ApiViewModelState): void {
        if (this._state !== value) {
            this._state = value;
            this.notifyPropertiesChanged('state');
        }
    }

    protected get(url: string): RequestHandlerBuilder {
        return this.request({ method: "GET", url });
    }

    protected post<TRequest>(url: string, data: TRequest): RequestHandlerBuilder {
        return this.request({ method: "POST", url, data });
    }

    protected put<TRequest>(url: string, data: TRequest): RequestHandlerBuilder {
        return this.request({ method: "PUT", url, data });
    }

    protected delete(url: string): RequestHandlerBuilder {
        return this.request({ method: "DELETE", url });
    }

    protected request(request: AxiosRequestConfig): RequestHandlerBuilder {
        return new RequestHandlerBuilder(request, this._axios)
            .onRequest((request, next) => {
                this._setState(ApiViewModelState.Busy);
                next(request);
            })
            .on(500, () => {
                this._alertsViewModel.addAlert('errors.internalServerError');
            })
            .onCompleted(() => {
                this._setState(ApiViewModelState.Ready);
            })
            .onFaulted(error => {
                this._setState(ApiViewModelState.Faulted);
                console.error(error);
            });
    }
}