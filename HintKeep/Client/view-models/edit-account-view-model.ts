import type { AxiosResponse } from 'axios';
import type { IEvent } from 'react-model-view-viewmodel';
import type { INotFoundResponseData as INotFoundGetResponseData, IResponseData as IGetResponseData } from '../api/accounts/get-by-id';
import type { IConflictResponseData, INotFoundResponseData as INotFoundPutResponseData, IRequestData, IResponseData as IPutResponseData, IUnprocessableEntityResponseData } from '../api/accounts/put';
import type { INotFoundResponseData as INotFoundDeleteResponseData, IResponseData as IDeleteResponseData } from '../api/accounts/delete';
import { DispatchEvent } from 'react-model-view-viewmodel';
import { ApiViewModel } from './api-view-model';
import { AccountForm } from './account-form';

export class EditAccountViewModel extends ApiViewModel {
    private _id: string | null = null;
    private _form: AccountForm = new AccountForm();
    private readonly _editedEvent: DispatchEvent = new DispatchEvent();
    private readonly _deletedEvent: DispatchEvent = new DispatchEvent();

    public get form(): AccountForm {
        return this._form;
    }

    public get isLoaded(): boolean {
        return this._id !== null;
    }

    public get editedEvent(): IEvent {
        return this._editedEvent;
    }

    public get deletedEvent(): IEvent {
        return this._deletedEvent;
    }

    public async loadAsync(id: string): Promise<void> {
        return this
            .get(`/api/accounts/${id}`)
            .on(200, ({ data: { name, hint, isPinned, notes } }: AxiosResponse<IGetResponseData>) => {
                this._id = id;
                this._form = new AccountForm();
                this._form.name.value = this._form.name.initialValue = name;
                this._form.hint.value = this._form.hint.initialValue = hint;
                this._form.isPinned.value = this._form.isPinned.initialValue = isPinned;
                this._form.notes.value = this._form.notes.initialValue = notes;
                this.notifyPropertiesChanged('isLoaded', 'form');
            })
            .on(404, (_: AxiosResponse<INotFoundGetResponseData>) => {
                this.alertsViewModel.addAlert('errors.accountNotFound');
            })
            .sendAsync();
    }

    public async submitAsync(): Promise<void> {
        if (this._id !== null) {
            this._form.fields.forEach(field => field.isTouched = true);
            if (this._form.isValid) {
                await this
                    .put<IRequestData>(`/api/accounts/${this._id}`, {
                        name: this._form.name.value,
                        hint: this._form.hint.value,
                        isPinned: this._form.isPinned.value,
                        notes: this._form.notes.value
                    })
                    .on(204, (_: AxiosResponse<IPutResponseData>) => {
                        this._editedEvent.dispatch(this);
                    })
                    .on(404, (_: AxiosResponse<INotFoundPutResponseData>) => {
                    })
                    .on(409, (_: AxiosResponse<IConflictResponseData>) => {
                        this._form.name.error = 'validation.errors.nameNotUnique';
                    })
                    .on(422, ({ data: { name: nameErrors, hint: hintErrors } }: AxiosResponse<IUnprocessableEntityResponseData>) => {
                        this._form.name.error = nameErrors[0];
                        this._form.hint.error = hintErrors[0];
                    })
                    .sendAsync();
            }
        }
    }

    public async deleteAsync(): Promise<any> {
        if (this._id !== null)
            await this
                .delete(`/api/accounts/${this._id}`)
                .on(204, (_: AxiosResponse<IDeleteResponseData>) => {
                    this._deletedEvent.dispatch(this);
                })
                .on(404, (_: AxiosResponse<INotFoundDeleteResponseData>) => {
                    this._deletedEvent.dispatch(this);
                })
                .sendAsync();
    }
}