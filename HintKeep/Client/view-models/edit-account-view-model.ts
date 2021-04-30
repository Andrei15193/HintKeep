import type { AxiosResponse } from 'axios';
import type { IFormField } from './core';
import type { IConflictResponseData, INotFoundResponseData as INotFoundPutResponseData, IRequestData, IResponseData as IPutResponseData, IUnprocessableEntityResponseData } from '../api/accounts/put';
import type { INotFoundResponseData as INotFoundGetResponseData, IResponseData as IGetResponseData } from '../api/accounts/get-by-id';
import { FormViewModel, FormField } from './core';
import { Axios } from '../services';
import { DispatchEvent, IEvent } from '../events';
import { alertsStore } from '../stores';

export class EditAccountViewModel extends FormViewModel {
    private _id: string | null;
    private readonly _editedEvent: DispatchEvent;
    private readonly _deletedEvent: DispatchEvent;
    private readonly _name: FormField<string>;
    private readonly _hint: FormField<string>;
    private readonly _isPinned: FormField<boolean>;
    private readonly _notes: FormField<string>;

    public constructor() {
        super(Axios);
        this._editedEvent = new DispatchEvent();
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

    public async submitAsync(): Promise<void> {
        if (this._id !== null) {
            this.touchAllFields();
            if (this.isValid) {
                await this
                    .put<IRequestData>(`/api/accounts/${this._id}`, {
                        name: this.name.value,
                        hint: this.hint.value,
                        isPinned: this.isPinned.value,
                        notes: this.notes.value
                    })
                    .on(204, (_: AxiosResponse<IPutResponseData>) => {
                        this._editedEvent.dispatch(this);
                    })
                    .on(404, (_: AxiosResponse<INotFoundPutResponseData>) => {
                    })
                    .on(409, (_: AxiosResponse<IConflictResponseData>) => {
                        this._name.errors = ['validation.errors.nameNotUnique'];
                    })
                    .on(422, ({ data: { name: nameErrors, hint: hintErrors } }: AxiosResponse<IUnprocessableEntityResponseData>) => {
                        this._name.errors = nameErrors;
                        this._hint.errors = hintErrors;
                    })
                    .sendAsync();
            }
        }
    }

    public async deleteAsync(): Promise<any> {
        if (this._id !== null)
            await this
                .delete(`/api/accounts/${this._id}`)
                .on(204, (_: AxiosResponse<IPutResponseData>) => {
                    this._deletedEvent.dispatch(this);
                })
                .on(404, (_: AxiosResponse<INotFoundPutResponseData>) => {
                    this._deletedEvent.dispatch(this);
                })
                .sendAsync();
    }

    protected fieldChanged(field: FormField<string>, changedProperties: readonly string[]): void {
        if (changedProperties.includes('value') || changedProperties.includes('isTouched'))
            switch (field) {
                case this._name:
                case this._hint:
                    field.errors = field.value?.length ? [] : ['validation.errors.required'];
                    break;
            }

        super.fieldChanged(field, changedProperties);
    }
}