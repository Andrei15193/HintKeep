import type { AxiosResponse } from 'axios';
import type { IFormField } from './core';
import type { INotFoundResponseData as INotFoundGetResponseData, IResponseData as IGetResponseData } from '../api/accounts/get-by-id';
import { FormViewModel, FormField } from './core';
import { Axios } from '../services';
import { DispatchEvent, IEvent } from '../events';
import { alertsStore } from '../stores';

export class DeletedAccountDetailsViewModel extends FormViewModel {
    private _id: string | null;
    private readonly _restoredEvent: DispatchEvent;
    private readonly _name: FormField<string>;
    private readonly _hint: FormField<string>;
    private readonly _isPinned: FormField<boolean>;

    constructor() {
        super(Axios);
        this._restoredEvent = new DispatchEvent();
        this._id = null;
        this.register(
            this._name = new FormField<string>(''),
            this._hint = new FormField<string>(''),
            this._isPinned = new FormField<boolean>(false)
        );
    }

    public get name(): IFormField<string> {
        return this._name;
    }

    public get hint(): IFormField<string> {
        return this._hint;
    }

    public get isPinned(): IFormField<boolean> {
        return this._isPinned;
    }

    public get isLoaded(): boolean {
        return this._id !== null;
    }

    public get restoredEvent(): IEvent {
        return this._restoredEvent;
    }

    public async loadAsync(id: string): Promise<void> {
        return this
            .get(`/api/accounts/${id}`)
            .on(200, ({ data: { name, hint, isPinned } }: AxiosResponse<IGetResponseData>) => {
                this._id = id;
                this._name.value = name;
                this._hint.value = hint;
                this._isPinned.value = isPinned;
                this.notifyPropertyChanged('isLoaded');
            })
            .on(404, (_: AxiosResponse<INotFoundGetResponseData>) => {
                alertsStore.addAlert('errors.accountNotFound');
            })
            .sendAsync();
    }
}