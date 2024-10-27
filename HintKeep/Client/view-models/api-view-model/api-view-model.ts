import type { AxiosInstance, AxiosRequestConfig } from 'axios';
import { type IDependencyResolver, ViewModel } from 'react-model-view-viewmodel';
import { AlertsViewModel } from '../alerts-view-model';
import { SessionViewModel } from '../session-view-model';
import { RequestHandlerBuilder } from './request-handler-builder';
import { Axios } from '../../services';

export enum ApiViewModelState {
    Ready,
    Busy,
    Faulted
}

export abstract class ApiViewModel extends ViewModel {
    private _state: ApiViewModelState;
    private readonly _axios: AxiosInstance;

    public constructor({ resolve }: IDependencyResolver) {
        super();
        this._state = ApiViewModelState.Ready;
        this._axios = resolve(Axios);
        this.alertsViewModel = resolve(AlertsViewModel);
        this.sessionViewModel = resolve(SessionViewModel);
    }

    public get state(): ApiViewModelState {
        return this._state;
    }

    protected readonly alertsViewModel: AlertsViewModel;

    protected readonly sessionViewModel: SessionViewModel;

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
            .on(401, () => {
                this.sessionViewModel.endSession();
            })
            .on(500, () => {
                this.alertsViewModel.addAlert('errors.internalServerError');
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