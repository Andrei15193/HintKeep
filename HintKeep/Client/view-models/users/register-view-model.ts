import type { AxiosResponse } from 'axios';
import type { IConflictResponseData, IRequestData, IResponseData, IUnprocessableEntityResponseData } from '../../api/users/post';
import { IEvent, registerValidators } from 'react-model-view-viewmodel';
import { DispatchEvent, FormFieldCollectionViewModel, FormFieldViewModel } from 'react-model-view-viewmodel';
import { ApiViewModel } from '../api-view-model';
import { required } from '../validation';

export class RegisterUserViewModel extends ApiViewModel {
    private readonly _registeredEvent: DispatchEvent = new DispatchEvent();

    public readonly form: RegisterUserFormViewModel = new RegisterUserFormViewModel();

    public get registered(): IEvent {
        return this._registeredEvent;
    }

    public async submitAsync(): Promise<void> {
        this.form.fields.forEach(field => field.isTouched = true);
        if (this.form.isValid)
            await this
                .post<IRequestData>('/api/users', {
                    email: this.form.email.value,
                    hint: this.form.hint.value,
                    password: this.form.password.value
                })
                .on(201, (_: AxiosResponse<IResponseData>) => {
                    this._registeredEvent.dispatch(this);
                })
                .on(409, (_: AxiosResponse<IConflictResponseData>) => {
                    this.form.email.error = 'validation.errors.emailNotUnique';
                })
                .on(422, ({ data: { email: emailErrors = [], hint: hintErrors = [], password: passwordErrors = [] } }: AxiosResponse<IUnprocessableEntityResponseData>) => {
                    this.form.email.error = emailErrors[0];
                    this.form.hint.error = hintErrors[0];
                    this.form.password.error = passwordErrors[0];
                })
                .sendAsync();
    }
}

class RegisterUserFormViewModel extends FormFieldCollectionViewModel {
    public constructor() {
        super();
        registerValidators(this.email = this.addField('email', ''), [required]);
        registerValidators(this.hint = this.addField('hint', ''), [required]);
        registerValidators(this.password = this.addField('password', ''), [required]);
        this.termsOfServiceAcceptance = this.addField('termsOfServiceAcceptance', false);

        this.fields.forEach(field => field.propertiesChanged.subscribe({ handle: this._fieldChanged }));
    }

    public readonly email: FormFieldViewModel<string>;

    public readonly hint: FormFieldViewModel<string>;

    public readonly password: FormFieldViewModel<string>;

    public readonly termsOfServiceAcceptance: FormFieldViewModel<boolean>;

    public get areAllFieldsTouched(): boolean {
        return this.fields.every(field => field.isTouched);
    }

    private _fieldChanged = (field: FormFieldViewModel<any>, changedProperties: readonly string[]): void => {
        if (changedProperties.includes('isTouched'))
            this.notifyPropertiesChanged('areAllFieldsTouched');
    };
}