import type { AxiosInstance, AxiosRequestConfig } from 'axios';
import type { INotifyPropertyChanged } from '../../events';
import { alertsStore, userStore } from '../../stores';
import { RequestHandlerBuilder } from './request-handler-builder';
import { ViewModel } from './view-model';

export enum ApiViewModelState {
    Ready,
    Busy,
    Faulted
}

export abstract class ApiViewModel extends ViewModel {
    private readonly _axios: AxiosInstance;
    private _state: ApiViewModelState = ApiViewModelState.Ready;

    public constructor(axios: AxiosInstance, ...stores: readonly INotifyPropertyChanged[]) {
        super(...stores);
        this._axios = axios;
    }

    public get state(): ApiViewModelState {
        return this._state;
    }

    private _setState(value: ApiViewModelState): void {
        if (this._state !== value) {
            this._state = value;
            this.notifyPropertyChanged('state');
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
                switch (request.url) {
                    case '/api/users/sessions':
                        break;

                    default:
                        alertsStore.addAlert('errors.sessionExpired');
                        userStore.completeSession();
                        break;
                }
            })
            .on(500, () => {
                alertsStore.addAlert('errors.internalServerError');
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