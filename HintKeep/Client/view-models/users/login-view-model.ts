import type { AxiosResponse } from 'axios';
import type { FormFieldViewModel, IEvent } from 'react-model-view-viewmodel';
import type { IRequestData, IResponseData, IUnprocessableEntityResponseData } from '../../api/users/authentications/post';
import { DispatchEvent, FormFieldCollectionViewModel, registerValidators } from 'react-model-view-viewmodel';
import { ApiViewModel } from '../api-view-model';
import { required } from '../validation';

export class LoginViewModel extends ApiViewModel {
    private readonly _authenticated: DispatchEvent = new DispatchEvent();

    public readonly form: LoginFormViewModel = new LoginFormViewModel();

    public get authenticated(): IEvent {
        return this._authenticated;
    }

    public async authenticateAsync(): Promise<void> {
        this.form.fields.forEach(field => field.isTouched = true);
        if (this.form.isValid)
            await this
                .post<IRequestData>('/api/users/sessions', {
                    email: this.form.email.value,
                    password: this.form.password.value
                })
                .on(404, () => {
                    this.form.password.error = 'errors.login.invalidCredentials';
                })
                .on(201, ({ data: jsonWebToken }: AxiosResponse<IResponseData>) => {
                    this.sessionViewModel.beginSession(jsonWebToken);
                    this._authenticated.dispatch(this);
                })
                .on(422, ({ data: { '*': errors = [], email: emailErrors = [], password: passwordErrors = [] } }: AxiosResponse<IUnprocessableEntityResponseData>) => {
                    this.form.email.error = emailErrors[0];
                    this.form.password.error = passwordErrors[0] || errors[0];
                })
                .sendAsync();
    }
}

class LoginFormViewModel extends FormFieldCollectionViewModel {
    public constructor() {
        super();
        registerValidators(this.email = this.addField('email', ''), [required]);
        registerValidators(this.password = this.addField('password', ''), [required]);

        this.fields.forEach(field => field.propertiesChanged.subscribe({ handle: this._fieldChanged }));
    }

    public readonly email: FormFieldViewModel<string>;

    public readonly password: FormFieldViewModel<string>;

    public get areAllFieldsTouched(): boolean {
        return this.fields.every(field => field.isTouched);
    }

    private _fieldChanged = (field: FormFieldViewModel<any>, changedProperties: readonly string[]): void => {
        if (changedProperties.includes('isTouched'))
            this.notifyPropertiesChanged('areAllFieldsTouched');
    };
}