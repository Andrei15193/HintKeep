import type { AxiosResponse } from "axios";
import type { INotFoundResponseData, IRequestData, IResponseData, IUnprocessableEntityResponseData } from "../../api/users/passwords/post";
import { type IEvent, EventDispatcher } from "react-model-view-viewmodel";
import { ApiViewModel } from "../api-view-model";
import { required } from "../validation";
import { HintKeepForm, HintKeepFormField } from "../forms";

export class PasswordResetViewModel extends ApiViewModel {
    private readonly _passwordReset: EventDispatcher<this> = new EventDispatcher<this>();

    public readonly form: PasswordResetFormViewModel = new PasswordResetFormViewModel();

    public get passwordReset(): IEvent<this> {
        return this._passwordReset;
    }

    public async resetPasswordAsync(): Promise<void> {
        this.form.fields.forEach(field => field.isTouched = true);
        if (this.form.isValid)
            await this
                .post<IRequestData>('/api/users/passwords', {
                    email: this.form.email.value,
                    token: this.form.token.value,
                    password: this.form.password.value
                })
                .on(201, (_: AxiosResponse<IResponseData>) => {
                    this._passwordReset.dispatch(this);
                })
                .on(404, (_: AxiosResponse<INotFoundResponseData>) => {
                    this.form.token.error = 'errors.passwordReset.tokenExpired';
                })
                .on(422, ({ data: { email: emailErrors = [], token: tokenErrors = [], password: passwordErrors = [] } }: AxiosResponse<IUnprocessableEntityResponseData>) => {
                    this.form.email.error = emailErrors[0];
                    this.form.token.error = tokenErrors[0];
                    this.form.password.error = passwordErrors[0];
                })
                .sendAsync();
    }
}

class PasswordResetFormViewModel extends HintKeepForm {
    public constructor() {
        super();

        this.withFields(
            this.email = new HintKeepFormField<string>({
                name: 'email',
                initialValue: '',
                validators: [required]
            }),
            this.token = new HintKeepFormField<string>({
                name: 'token',
                initialValue: '',
                validators: [required]
            }),
            this.password = new HintKeepFormField<string>({
                name: 'password',
                initialValue: '',
                validators: [required]
            }),
            this.passwordConfirmation = new HintKeepFormField<string>({
                name: 'passwordConfirmation',
                initialValue: '',
                validators: [
                    required,
                    passwordConfirmation => passwordConfirmation.value !== this.password.value ? 'validation.errors.passwordsDoNotMatch' : undefined
                ],
                validationTriggers: [
                    this.password
                ]
            })
        )
    }

    public readonly email: HintKeepFormField<string>;

    public readonly token: HintKeepFormField<string>;

    public readonly password: HintKeepFormField<string>;

    public readonly passwordConfirmation: HintKeepFormField<string>;
}