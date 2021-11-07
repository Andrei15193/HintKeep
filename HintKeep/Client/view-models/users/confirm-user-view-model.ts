import type { AxiosResponse } from 'axios';
import type { FormFieldViewModel, IEvent } from 'react-model-view-viewmodel';
import type { INotFoundResponseData, IRequestData, IResponseData, IUnprocessableEntityResponseData } from '../../api/users/confirmations/post';
import { DispatchEvent, FormFieldCollectionViewModel, registerValidators } from 'react-model-view-viewmodel';
import { ApiViewModel } from '../api-view-model';
import { required } from '../validation';

export class ConfirmUserViewModel extends ApiViewModel {
    private readonly _confirmedEvent: DispatchEvent = new DispatchEvent();

    public readonly form: ConfirmUserFormViewModel = new ConfirmUserFormViewModel();

    public get confirmed(): IEvent {
        return this._confirmedEvent;
    }

    public async confirmAsync(): Promise<void> {
        this.form.fields.forEach(field => field.isTouched = true);
        if (this.form.isValid)
            await this
                .post<IRequestData>('/api/users/confirmations', {
                    token: this.form.token.value
                })
                .on(201, (_: AxiosResponse<IResponseData>) => {
                    this._confirmedEvent.dispatch(this);
                })
                .on(404, (_: AxiosResponse<INotFoundResponseData>) => {
                    this.form.token.error = 'errors.confirmation.tokenExpired';
                })
                .on(422, ({ data: { token: tokenError = [] } }: AxiosResponse<IUnprocessableEntityResponseData>) => {
                    this.form.token.error = tokenError[0];
                })
                .sendAsync();
    }
}

class ConfirmUserFormViewModel extends FormFieldCollectionViewModel {
    public constructor() {
        super();
        registerValidators(this.token = this.addField('token', ''), [required]);

        this.fields.forEach(field => field.propertiesChanged.subscribe({ handle: this._fieldChanged }));
    }

    public readonly token: FormFieldViewModel<string>;

    public get areAllFieldsTouched(): boolean {
        return this.fields.every(field => field.isTouched);
    }

    private _fieldChanged = (field: FormFieldViewModel<any>, changedProperties: readonly string[]): void => {
        if (changedProperties.includes('isTouched'))
            this.notifyPropertiesChanged('areAllFieldsTouched');
    };
}