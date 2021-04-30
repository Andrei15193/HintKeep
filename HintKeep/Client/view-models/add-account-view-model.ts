import type { AxiosResponse } from 'axios';
import type { IConflictResponseData, IRequestData, IResponseData, IUnprocessableEntityResponseData } from '../api/accounts/post';
import type { FormFieldViewModel } from './core'
import { ApiViewModel, FormFieldCollectionViewModel, combineValidators, required } from './core';
import { Axios } from '../services';
import { DispatchEvent, IEvent } from '../events';

export class AddAccountViewModel extends ApiViewModel {
    private readonly _submittedEvent: DispatchEvent = new DispatchEvent();

    public constructor() {
        super(Axios);
    }

    public readonly form: AddAccountForm = new AddAccountForm();

    public get submittedEvent(): IEvent {
        return this._submittedEvent;
    }

    public async submitAsync(): Promise<void> {
        this.form.fields.forEach(field => field.isTouched = true);
        if (this.form.isValid) {
            await this
                .post<IRequestData>('/api/accounts', {
                    name: this.form.name.value,
                    hint: this.form.hint.value,
                    isPinned: this.form.isPinned.value,
                    notes: this.form.notes.value
                })
                .on(201, (_: AxiosResponse<IResponseData>) => {
                    this._submittedEvent.dispatch(this);
                })
                .on(409, (_: AxiosResponse<IConflictResponseData>) => {
                    this.form.name.errors = ['validation.errors.nameNotUnique'];
                })
                .on(422, ({ data: { name: nameErrors, hint: hintErrors } }: AxiosResponse<IUnprocessableEntityResponseData>) => {
                    this.form.name.errors = nameErrors;
                    this.form.hint.errors = hintErrors;
                })
                .sendAsync();
        }
    }
}

class AddAccountForm extends FormFieldCollectionViewModel {
    public constructor() {
        super();
        this.name = this.addField('', combineValidators(required), this._fieldChanged);
        this.hint = this.addField('', combineValidators(required), this._fieldChanged);
        this.isPinned = this.addField(false, this._fieldChanged);
        this.notes = this.addField('', this._fieldChanged);
    }

    public readonly name: FormFieldViewModel<string>;
    public readonly hint: FormFieldViewModel<string>;
    public readonly isPinned: FormFieldViewModel<boolean>;
    public readonly notes: FormFieldViewModel<string>;

    public get areAllFieldsTouched(): boolean {
        return this.fields.every(field => field.isTouched);
    }

    private _fieldChanged(field: FormFieldViewModel<any>, changedProperties: readonly string[]): void {
        if (changedProperties.includes('isTouched'))
            this.notifyPropertyChanged('areAllFieldsTouched');
    }
}