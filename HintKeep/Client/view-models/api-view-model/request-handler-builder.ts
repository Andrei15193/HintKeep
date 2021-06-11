import type { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios';

export type RequestHandlerCallback = (request: AxiosRequestConfig) => void;
export type RequestMiddlewareCallback = (request: AxiosRequestConfig, next: RequestHandlerCallback) => void;

export type ResponseHandlerCallback = (response: AxiosResponse) => void;
export type ResponseMiddlewareCallback = (response: AxiosResponse, next: ResponseHandlerCallback) => void;

export type ErrorHandlerCallback = (error: any) => void;
export type ErrorMiddlewareCallback = (error: any, next: ErrorHandlerCallback) => void;

export class RequestHandlerBuilder {
    private readonly _axios: AxiosInstance;
    private readonly _request: AxiosRequestConfig;
    private readonly _requestMiddleware: RequestMiddlewareCallback[] = [];
    private readonly _responseMiddleware: ResponseMiddlewareCallback[] = [];
    private readonly _successfulCompletionMiddleware: ResponseMiddlewareCallback[] = [];
    private readonly _faultedCompletionMiddleware: ErrorMiddlewareCallback[] = [];

    public constructor(request: AxiosRequestConfig, axios: AxiosInstance) {
        this._axios = axios;
        this._request = request;
    }

    public onCompleted(callback: ResponseMiddlewareCallback): RequestHandlerBuilder {
        this._successfulCompletionMiddleware.push(callback);
        return this;
    }

    public onFaulted(callback: ErrorMiddlewareCallback): RequestHandlerBuilder {
        this._faultedCompletionMiddleware.push(callback);
        return this;
    }

    public onRequest(requestMiddlewareCallback: RequestMiddlewareCallback): RequestHandlerBuilder {
        this._requestMiddleware.push(requestMiddlewareCallback);
        return this;
    }

    public onResponse(responseMiddlewareCallback: ResponseMiddlewareCallback): RequestHandlerBuilder {
        this._responseMiddleware.push(responseMiddlewareCallback)
        return this;
    }

    public on(httpStatusCode: number, callback: ResponseHandlerCallback): RequestHandlerBuilder {
        return this.onResponse((response, next) => {
            if (response.status === httpStatusCode)
                callback(response);
            else
                next(response);
        });
    }

    public sendAsync(): Promise<void> {
        return new Promise<AxiosResponse>(
            (resolve, reject) => {
                const requestHandler = this._requestMiddleware.reduceRight<RequestHandlerCallback>(
                    (nextCallback, currentCallback) => (request => currentCallback(request, nextCallback)),
                    (request: AxiosRequestConfig) => this
                        ._axios
                        .request(request)
                        .then(resposne => {
                            const responseHandler = this._responseMiddleware.reduceRight<ResponseHandlerCallback>(
                                (nextCallback, middlewareCallback) => (response => middlewareCallback(response, nextCallback)),
                                response => reject(new Error(`Unhandled response (${response.status}).\n${response.data}`))
                            );
                            responseHandler(resposne);
                            resolve(resposne);
                        })
                );
                requestHandler(this._request);
            })
            .then(response => {
                const successfulResponseHandler = this._successfulCompletionMiddleware.reduceRight<ResponseHandlerCallback>(
                    (nextCallback, middlewareCallback) => (response => middlewareCallback(response, nextCallback)),
                    () => { }
                );
                successfulResponseHandler(response);
            })
            .catch(error => {
                const faultedResponseHandler = this._faultedCompletionMiddleware.reduceRight<ResponseHandlerCallback>(
                    (nextCallback, middlewareCallback) => (response => middlewareCallback(response, nextCallback)),
                    error => { throw error; }
                );
                faultedResponseHandler(error);
            });
    }
}