import type { AxiosResponse } from 'axios';
import type { IRequestData, IResponseData, IUnprocessableEntityResponseData } from '../../api/users/authentications/post';
import { type IEvent, EventDispatcher } from 'react-model-view-viewmodel';
import { ApiViewModel } from '../api-view-model';
import { required } from '../validation';
import { HintKeepForm, HintKeepFormField } from '../forms';

export class LoginViewModel extends ApiViewModel {
    private readonly _authenticated: EventDispatcher<this> = new EventDispatcher<this>();

    public readonly form: LoginFormViewModel = new LoginFormViewModel();

    public get authenticated(): IEvent<this> {
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

class LoginFormViewModel extends HintKeepForm {
    public constructor() {
        super();

        this.withFields(
            this.email = new HintKeepFormField({
                name: 'email',
                initialValue: '',
                validators: [required]
            }),
            this.password = new HintKeepFormField({
                name: 'password',
                initialValue: '',
                validators: [required]
            })
        );
    }

    public readonly email: HintKeepFormField<string>;

    public readonly password: HintKeepFormField<string>;
}