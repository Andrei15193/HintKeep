import type { AxiosResponse } from 'axios';
import type { IEvent } from 'react-model-view-viewmodel';
import type { INotFoundResponseData as INotFoundGetResponseData, IResponseData as IGetResponseData } from '../api/deleted-accounts/get-by-id';
import type { INotFoundResponseData as INotFoundPutResponseData, IResponseData as IPutResponseData, IRequestData as IPutRequestData } from '../api/deleted-accounts/put';
import type { INotFoundResponseData as INotFoundDeleteResponseData, IResponseData as IDeleteResponseData } from '../api/deleted-accounts/delete';
import { DispatchEvent } from 'react-model-view-viewmodel';
import { ApiViewModel } from './api-view-model';
import { AccountForm } from './account-form';

export class DeletedAccountDetailsViewModel extends ApiViewModel {
    private _id: string | null = null;
    private _form: AccountForm = new AccountForm();
    private readonly _restoredEvent: DispatchEvent= new DispatchEvent();
    private readonly _deletedEvent: DispatchEvent= new DispatchEvent();

    public get form(): AccountForm {
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
                this._form = new AccountForm();
                this._form.name.initialValue = this._form.name.value = name;
                this._form.hint.initialValue = this._form.hint.value = hint;
                this._form.isPinned.initialValue = this._form.isPinned.value = isPinned;
                this._form.notes.initialValue = this._form.notes.value = notes;
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