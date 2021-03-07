import type { AxiosInstance, AxiosRequestConfig } from 'axios';
import type { INotifyPropertyChanged } from '../../events';
import { alertsStore } from '../../stores';
import { RequestHandlerBuilder } from './request-handler-builder';
import { ViewModel } from './view-model';

export enum ApiViewModelState {
    Ready,
    Busy,
    Faulted
}

export class ApiViewModel extends ViewModel {
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

    protected post(url: string, data: any): RequestHandlerBuilder {
        return this.request({ method: "POST", url, data });
    }

    protected put(url: string, data: any): RequestHandlerBuilder {
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
            .on(500, () => alertsStore.addAlert('Something went wrong with the server. Please refresh the page and try again.'))
            .onCompleted(() => {
                this._setState(ApiViewModelState.Ready);
            })
            .onFaulted(error => {
                this._setState(ApiViewModelState.Faulted);
                console.error(error);
            });
    }

    protected async delay(millisecondsDelay: number): Promise<void> {
        return new Promise(resolve => setTimeout(() => resolve(), millisecondsDelay));
    }

    protected async delayWithResult<TResult>(millisecondsDelay: number, result: TResult): Promise<TResult> {
        return new Promise(resolve => setTimeout(() => resolve(result), millisecondsDelay));
    }
}