import type { AxiosResponse } from 'axios';
import type { IFormField } from './core';
import type { IConflictResponseData, IRequestData, IResponseData, IUnprocessableEntityResponseData } from '../api/accounts/post';
import { FormViewModel, FormField } from './core';
import { Axios } from '../services';
import { DispatchEvent, IEvent } from '../events';

export class AddAccountViewModel extends FormViewModel {
    private readonly _submittedEvent: DispatchEvent;
    private readonly _name: FormField<string>;
    private readonly _hint: FormField<string>;
    private readonly _isPinned: FormField<boolean>;

    constructor() {
        super(Axios);
        this._submittedEvent = new DispatchEvent();
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

    public get submittedEvent(): IEvent {
        return this._submittedEvent;
    }

    public async submitAsync(): Promise<void> {
        this.touchAllFields();
        if (this.isValid) {
            await this
                .post<IRequestData>('/api/accounts', {
                    name: this.name.value,
                    hint: this.hint.value,
                    isPinned: this.isPinned.value
                })
                .on(201, (_: AxiosResponse<IResponseData>) => {
                    this._submittedEvent.dispatch(this);
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