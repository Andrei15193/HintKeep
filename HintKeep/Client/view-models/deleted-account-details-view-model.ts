import type { AxiosResponse } from 'axios';
import type { IEvent } from 'react-model-view-viewmodel';
import type { AlertsViewModel } from './alerts-view-model';
import type { INotFoundResponseData as INotFoundGetResponseData, IResponseData as IGetResponseData } from '../api/deleted-accounts/get-by-id';
import type { INotFoundResponseData as INotFoundPutResponseData, IResponseData as IPutResponseData, IRequestData as IPutRequestData } from '../api/deleted-accounts/put';
import type { INotFoundResponseData as INotFoundDeleteResponseData, IResponseData as IDeleteResponseData } from '../api/deleted-accounts/delete';
import { FormFieldCollectionViewModel, FormFieldViewModel, DispatchEvent } from 'react-model-view-viewmodel';
import { Axios } from '../services';
import { ApiViewModel } from './api-view-model';

export class DeletedAccountDetailsViewModel extends ApiViewModel {
    private _id: string | null;
    private readonly _restoredEvent: DispatchEvent;
    private readonly _deletedEvent: DispatchEvent;
    private _form: DeletedAccountFormViewModel;

    public constructor(alertsViewModel: AlertsViewModel) {
        super(Axios, alertsViewModel);
        this._restoredEvent = new DispatchEvent();
        this._deletedEvent = new DispatchEvent();
        this._id = null;
        this._form = new DeletedAccountFormViewModel();
    }

    public get form(): DeletedAccountFormViewModel {
        return this._form;
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
                this._form = new DeletedAccountFormViewModel({ name, hint, isPinned, notes });
                this.notifyPropertiesChanged('isLoaded');
            })
            .on(404, (_: AxiosResponse<INotFoundGetResponseData>) => {
                this.alertsViewModel.addAlert('errors.accountNotFound');
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
                    this.alertsViewModel.addAlert('errors.accountNotFound');
                })
                .sendAsync();
    }

    public async deleteAsync(): Promise<any> {
        if (this._id !== null)
            await this
                .delete(`/api/deleted-accounts/${this._id}`)
                .on(204, (_: AxiosResponse<IDeleteResponseData>) => {
                    this._deletedEvent.dispatch(this);
                })
                .on(404, (_: AxiosResponse<INotFoundDeleteResponseData>) => {
                    this._deletedEvent.dispatch(this);
                })
                .sendAsync();
    }
}

export interface IDeleteAccountFormFieldValues {
    readonly name: string;
    readonly hint: string;
    readonly isPinned: boolean;
    readonly notes: string;
}

export class DeletedAccountFormViewModel extends FormFieldCollectionViewModel {
    public constructor(fields?: IDeleteAccountFormFieldValues) {
        super();
        this.name = this.addField(fields && fields.name || '');
        this.hint = this.addField(fields && fields.hint || '');
        this.isPinned = this.addField(fields && fields.isPinned || false);
        this.notes = this.addField(fields && fields.notes || '');
    }

    public name: FormFieldViewModel<string>;

    public hint: FormFieldViewModel<string>;

    public isPinned: FormFieldViewModel<boolean>;

    public notes: FormFieldViewModel<string>;
}