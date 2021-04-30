import type { AxiosResponse } from 'axios';
import type { IFormField } from './core';
import type { INotFoundResponseData as INotFoundGetResponseData, IResponseData as IGetResponseData } from '../api/deleted-accounts/get-by-id';
import type { INotFoundResponseData as INotFoundPutResponseData, IResponseData as IPutResponseData, IRequestData as IPutRequestData } from '../api/deleted-accounts/put';
import { FormViewModel, FormField } from './core';
import { Axios } from '../services';
import { DispatchEvent, IEvent } from '../events';
import { alertsStore } from '../stores';

export class DeletedAccountDetailsViewModel extends FormViewModel {
    private _id: string | null;
    private readonly _restoredEvent: DispatchEvent;
    private readonly _deletedEvent: DispatchEvent;
    private readonly _name: FormField<string>;
    private readonly _hint: FormField<string>;
    private readonly _isPinned: FormField<boolean>;
    private readonly _notes: FormField<string>;

    public constructor() {
        super(Axios);
        this._restoredEvent = new DispatchEvent();
        this._deletedEvent = new DispatchEvent();
        this._id = null;
        this.register(
            this._name = new FormField<string>(''),
            this._hint = new FormField<string>(''),
            this._isPinned = new FormField<boolean>(false),
            this._notes = new FormField<string>('')
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

    public get notes(): IFormField<string> {
        return this._notes;
    }

    public get isLoaded(): boolean {
        return this._id !== null;
    }

    public get restoredEvent(): IEvent {
        return this._restoredEvent;
    }

    public get deletedEvent(): IEvent {
        return this._deletedEvent;
    }

    public async loadAsync(id: string): Promise<void> {
        return this
            .get(`/api/deleted-accounts/${id}`)
            .on(200, ({ data: { name, hint, isPinned, notes } }: AxiosResponse<IGetResponseData>) => {
                this._id = id;
                this._name.value = name;
                this._hint.value = hint;
                this._isPinned.value = isPinned;
                this._notes.value = notes;
                this.notifyPropertyChanged('isLoaded');
            })
            .on(404, (_: AxiosResponse<INotFoundGetResponseData>) => {
                alertsStore.addAlert('errors.accountNotFound');
            })
            .sendAsync();
    }

    public async restoreAsync(): Promise<void> {
        if (this.isLoaded)
            await this
                .put<IPutRequestData>(`/api/deleted-accounts/${this._id}`, { isDeleted: false })
                .on(204, (_: AxiosResponse<IPutResponseData>) => {
                    this._restoredEvent.dispatch(this);
                })
                .on(404, (_: AxiosResponse<INotFoundPutResponseData>) => {
                    alertsStore.addAlert('errors.accountNotFound');
                })
                .sendAsync();
    }

    public async deleteAsync(): Promise<any> {
        if (this._id !== null)
            await this
                .delete(`/api/deleted-accounts/${this._id}`)
                .on(204, (_: AxiosResponse<IPutResponseData>) => {
                    this._deletedEvent.dispatch(this);
                })
                .on(404, (_: AxiosResponse<INotFoundPutResponseData>) => {
                    this._deletedEvent.dispatch(this);
                })
                .sendAsync();
    }
}