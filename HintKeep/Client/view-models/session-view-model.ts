import { type AxiosInstance } from "axios";
import { type IDependencyResolver, ViewModel } from "react-model-view-viewmodel";
import { Axios } from "../services";

export class SessionViewModel extends ViewModel {
    private _isSessionActive: boolean;
    private readonly _axios: AxiosInstance;

    public constructor({ resolve }: IDependencyResolver) {
        super();

        this._isSessionActive = false;
        this._axios = resolve(Axios);
    }

    public get isSessionActive(): boolean {
        return this._isSessionActive;
    }

    public beginSession(jsonWebToken: string): void {
        Object.assign(this._axios.defaults.headers, { Authorization: `Bearer ${jsonWebToken}` });
        this._isSessionActive = true;
        this.notifyPropertiesChanged("isSessionActive");
    }

    public endSession(): void {
        if (this._axios.defaults.headers) {
            delete this._axios.defaults.headers.common.Authorization;
            this._isSessionActive = false;
            this.notifyPropertiesChanged("isSessionActive");
        }
    }
}