import type { AxiosResponse } from 'axios';
import type { IEvent } from 'react-model-view-viewmodel'
import type { AlertsViewModel } from './alerts-view-model';
import type { IConflictResponseData, IRequestData, IResponseData, IUnprocessableEntityResponseData } from '../api/accounts/post';
import { FormFieldViewModel, FormFieldCollectionViewModel, registerValidators, DispatchEvent } from 'react-model-view-viewmodel'
import { ApiViewModel } from './api-view-model';
import { required } from './validation';
import { Axios } from '../services';

export class AddAccountViewModel extends ApiViewModel {
    private readonly _submittedEvent: DispatchEvent = new DispatchEvent();

    public constructor(alertsViewModel: AlertsViewModel) {
        super(Axios, alertsViewModel);
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
                    this.form.name.error = 'validation.errors.nameNotUnique';
                })
                .on(422, ({ data: { name: nameErrors, hint: hintErrors } }: AxiosResponse<IUnprocessableEntityResponseData>) => {
                    this.form.name.error = nameErrors[0];
                    this.form.hint.error = hintErrors[0];
                })
                .sendAsync();
        }
    }
}

class AddAccountForm extends FormFieldCollectionViewModel {
    public constructor() {
        super();
        registerValidators(this.name = this.addField('name', ''), [required]);
        registerValidators(this.hint = this.addField('hint', ''), [required]);
        this.isPinned = this.addField('isPinned', false);
        this.notes = this.addField('notes', '');

        this.fields.forEach(field => field.propertiesChanged.subscribe({ handle: this._fieldChanged }));
    }

    public readonly name: FormFieldViewModel<string>;
    public readonly hint: FormFieldViewModel<string>;
    public readonly isPinned: FormFieldViewModel<boolean>;
    public readonly notes: FormFieldViewModel<string>;

    public get areAllFieldsTouched(): boolean {
        return this.fields.every(field => field.isTouched);
    }

    private _fieldChanged = (field: FormFieldViewModel<any>, changedProperties: readonly string[]): void => {
        if (changedProperties.includes('isTouched'))
            this.notifyPropertiesChanged('areAllFieldsTouched');
    }
}